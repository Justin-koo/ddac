using Amazon.S3;
using Amazon.S3.Model;
using System;
using Amazon;

public class S3Service
{
	private readonly AmazonS3Client _s3Client;
	private const string BucketName = "webapp-prod-image";

	public S3Service(IConfiguration configuration)
	{
		var region = RegionEndpoint.GetBySystemName(configuration["AWS:Region"]);
		_s3Client = new AmazonS3Client(region);
	}

	public async Task<List<string>> UploadFilesToS3(List<IFormFile> files, string folderPath)
	{
		var uploadUrls = new List<string>();

		foreach (var file in files)
		{
			if (file.Length > 0)
			{
				var keyName = $"{folderPath}/{Guid.NewGuid()}";
				using var newMemoryStream = new MemoryStream();
				await file.CopyToAsync(newMemoryStream);

				var uploadRequest = new PutObjectRequest
				{
					InputStream = newMemoryStream,
					BucketName = BucketName,  // Use the constant bucket name
					Key = keyName,
					ContentType = file.ContentType
				};

				newMemoryStream.Position = 0; // Reset the position after copying to stream
				var response = await _s3Client.PutObjectAsync(uploadRequest);
				if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
				{
					uploadUrls.Add(keyName);  // Store the key for later retrieval
				}
			}
		}

		return uploadUrls;
	}

	public async Task<bool> DeleteFileFromS3(string keyName)
	{
		try
		{
			var deleteObjectRequest = new DeleteObjectRequest
			{
				BucketName = BucketName,  // This is your S3 bucket
				Key = keyName             // This is the exact path of the file in S3
			};

			var response = await _s3Client.DeleteObjectAsync(deleteObjectRequest);

			// Check if the response indicates a successful deletion
			return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
		}
		catch (AmazonS3Exception e)
		{
			// Log or handle exception as needed
			Console.WriteLine($"Error encountered on server. Message:'{e.Message}' when deleting an object");
			return false;
		}
		catch (Exception e)
		{
			// Log or handle exception as needed
			Console.WriteLine($"Unknown error encountered on server. Message:'{e.Message}' when deleting an object");
			return false;
		}
	}


	public string GeneratePresignedURL(string objectKey, int durationInMinutes = 60)
	{
		var request = new GetPreSignedUrlRequest
		{
			BucketName = BucketName,
			Key = objectKey,
			Expires = DateTime.UtcNow.AddMinutes(durationInMinutes)
		};

		return _s3Client.GetPreSignedURL(request);
	}

}
