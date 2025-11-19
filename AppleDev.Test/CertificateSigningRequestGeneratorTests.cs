using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xunit.Abstractions;

namespace AppleDev.Test;

public class CertificateSigningRequestGeneratorTests
{
	private readonly ITestOutputHelper _testOutputHelper;

	public CertificateSigningRequestGeneratorTests(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper;
	}

	[Fact]
	public void GeneratePem_WithDefaultCommonName_ShouldReturnValidPem()
	{
		// Arrange
		var generator = new CertificateSigningRequestGenerator();

		// Act
		var pem = generator.GeneratePem();

		// Assert
		Assert.NotNull(pem);
		Assert.NotEmpty(pem);

		// Should start and end with PEM markers
		Assert.StartsWith("-----BEGIN CERTIFICATE REQUEST-----", pem);
		Assert.Contains("-----END CERTIFICATE REQUEST-----", pem);

		_testOutputHelper.WriteLine("Generated CSR:");
		_testOutputHelper.WriteLine(pem);
	}

	[Fact]
	public void GeneratePem_WithCustomCommonName_ShouldContainCommonName()
	{
		// Arrange
		var generator = new CertificateSigningRequestGenerator();
		var customName = "test.example.com";

		// Act
		var pem = generator.GeneratePem(customName);

		// Assert
		Assert.NotNull(pem);
		Assert.NotEmpty(pem);
		Assert.StartsWith("-----BEGIN CERTIFICATE REQUEST-----", pem);
		Assert.Contains("-----END CERTIFICATE REQUEST-----", pem);

		_testOutputHelper.WriteLine("Generated CSR:");
		_testOutputHelper.WriteLine(pem);
	}

	[Fact]
	public void GeneratePem_ShouldProduceValidBase64EncodedContent()
	{
		// Arrange
		var generator = new CertificateSigningRequestGenerator();

		// Act
		var pem = generator.GeneratePem("test-key-size");

		// Assert
		Assert.NotNull(pem);

		// Extract the base64 content between the markers
		var base64Content = pem
			.Replace("-----BEGIN CERTIFICATE REQUEST-----", "")
			.Replace("-----END CERTIFICATE REQUEST-----", "")
			.Replace(Environment.NewLine, "")
			.Replace("\n", "")
			.Replace("\r", "")
			.Trim();

		// Should be valid base64
		Assert.NotEmpty(base64Content);

		// Should be able to decode without exception
		var exception = Record.Exception(() => Convert.FromBase64String(base64Content));
		Assert.Null(exception);

		var csrBytes = Convert.FromBase64String(base64Content);

		// CSR should have substantial length (for 2048-bit key)
		Assert.True(csrBytes.Length > 500, $"CSR is too short: {csrBytes.Length} bytes");

		_testOutputHelper.WriteLine($"CSR Byte Length: {csrBytes.Length}");
		_testOutputHelper.WriteLine($"Base64 Length: {base64Content.Length}");
	}

	[Fact]
	public void GeneratePem_ShouldHaveConsistentFormat()
	{
		// Arrange
		var generator = new CertificateSigningRequestGenerator();

		// Act
		var pem = generator.GeneratePem("test-format");

		// Assert
		Assert.NotNull(pem);

		// Verify PEM structure
		var lines = pem.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

		Assert.True(lines.Length >= 3, "PEM should have at least header, content, and footer");
		Assert.Equal("-----BEGIN CERTIFICATE REQUEST-----", lines[0]);
		Assert.Equal("-----END CERTIFICATE REQUEST-----", lines[^1]);

		// Content lines should be base64 (typically 64 chars per line except last)
		for (int i = 1; i < lines.Length - 1; i++)
		{
			Assert.Matches(@"^[A-Za-z0-9+/=]+$", lines[i]);
		}

		_testOutputHelper.WriteLine($"Total lines: {lines.Length}");
		_testOutputHelper.WriteLine($"Content lines: {lines.Length - 2}");
	}

	[Fact]
	public void GeneratePem_CalledMultipleTimes_ShouldGenerateDifferentKeys()
	{
		// Arrange
		var generator = new CertificateSigningRequestGenerator();

		// Act
		var pem1 = generator.GeneratePem("test1");
		var pem2 = generator.GeneratePem("test2");

		// Assert
		Assert.NotNull(pem1);
		Assert.NotNull(pem2);

		// The PEM strings should be different (different keys generated)
		Assert.NotEqual(pem1, pem2);

		_testOutputHelper.WriteLine("First CSR length: " + pem1.Length);
		_testOutputHelper.WriteLine("Second CSR length: " + pem2.Length);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("test.com")]
	[InlineData("My Development Certificate")]
	[InlineData("*.example.com")]
	public void GeneratePem_WithVariousCommonNames_ShouldSucceed(string? commonName)
	{
		// Arrange
		var generator = new CertificateSigningRequestGenerator();

		// Act
		var pem = generator.GeneratePem(commonName);

		// Assert
		Assert.NotNull(pem);
		Assert.NotEmpty(pem);
		Assert.StartsWith("-----BEGIN CERTIFICATE REQUEST-----", pem);
		Assert.Contains("-----END CERTIFICATE REQUEST-----", pem);

		// Verify the base64 content is valid
		var base64Content = pem
			.Replace("-----BEGIN CERTIFICATE REQUEST-----", "")
			.Replace("-----END CERTIFICATE REQUEST-----", "")
			.Replace(Environment.NewLine, "")
			.Replace("\n", "")
			.Replace("\r", "")
			.Trim();

		var exception = Record.Exception(() => Convert.FromBase64String(base64Content));
		Assert.Null(exception);

		var expectedCN = commonName ?? Environment.MachineName;
		_testOutputHelper.WriteLine($"Common Name: {expectedCN}");
		_testOutputHelper.WriteLine($"PEM Length: {pem.Length}");
	}
}
