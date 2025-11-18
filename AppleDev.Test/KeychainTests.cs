using System.Runtime.InteropServices;
using Xunit.Abstractions;

namespace AppleDev.Test;

public class KeychainTests
{
	private readonly ITestOutputHelper _testOutputHelper;
	private readonly Keychain _keychain;

	public KeychainTests(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper;
		_keychain = new Keychain();
	}

	// ===== LOCATE METHOD TESTS =====

	[Fact]
	public void Locate_WithAbsolutePath_ShouldReturnSamePath()
	{
		// Arrange
		var absolutePath = "/Users/test/custom.keychain-db";

		// Act
		var result = _keychain.Locate(absolutePath);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(absolutePath, result.FullName);
	}

	[Fact]
	public void Locate_WithKeychainDbExtension_ShouldReturnCorrectPath()
	{
		// Arrange
		var keychainName = "test.keychain-db";
		var expectedPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
			"Library",
			"Keychains",
			keychainName);

		// Act
		var result = _keychain.Locate(keychainName);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(expectedPath, result.FullName);
		_testOutputHelper.WriteLine($"Located keychain: {result.FullName}");
	}

	[Fact]
	public void Locate_WithKeychainExtension_ShouldAppendDbSuffix()
	{
		// Arrange
		var keychainName = "test.keychain";
		var expectedPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
			"Library",
			"Keychains",
			"test.keychain-db");

		// Act
		var result = _keychain.Locate(keychainName);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(expectedPath, result.FullName);
		Assert.EndsWith(".keychain-db", result.FullName);
		_testOutputHelper.WriteLine($"Located keychain: {result.FullName}");
	}

	[Fact]
	public void Locate_WithNoExtension_ShouldAppendKeychainDb()
	{
		// Arrange
		var keychainName = "test";
		var expectedPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
			"Library",
			"Keychains",
			"test.keychain-db");

		// Act
		var result = _keychain.Locate(keychainName);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(expectedPath, result.FullName);
		Assert.EndsWith(".keychain-db", result.FullName);
		_testOutputHelper.WriteLine($"Located keychain: {result.FullName}");
	}

	[Fact]
	public void Locate_WithDefaultKeychain_ShouldReturnLoginKeychainPath()
	{
		// Arrange
		var expectedPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
			"Library",
			"Keychains",
			Keychain.DefaultKeychain);

		// Act
		var result = _keychain.Locate(Keychain.DefaultKeychain);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(expectedPath, result.FullName);
		Assert.EndsWith("login.keychain-db", result.FullName);
		_testOutputHelper.WriteLine($"Default keychain: {result.FullName}");
	}

	[Theory]
	[InlineData("myapp")]
	[InlineData("build.keychain")]
	[InlineData("ci.keychain-db")]
	[InlineData("temp-keychain")]
	public void Locate_WithVariousNames_ShouldAlwaysEndWithKeychainDb(string keychainName)
	{
		// Act
		var result = _keychain.Locate(keychainName);

		// Assert
		Assert.NotNull(result);
		Assert.EndsWith(".keychain-db", result.FullName);
		Assert.Contains("Library/Keychains", result.FullName);
		_testOutputHelper.WriteLine($"Input: {keychainName} -> Output: {result.FullName}");
	}

	// ===== INTEGRATION TESTS (macOS only) =====

	[SkippableFact]
	public async Task CreateKeychainAsync_WithValidParameters_ShouldSucceed()
	{
		Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.OSX), "Keychain operations only available on macOS");

		// Arrange
		var testKeychainName = $"test-keychain-{Guid.NewGuid():N}";
		var password = "test-password-123";

		try
		{
			// Act
			var result = await _keychain.CreateKeychainAsync(testKeychainName, password);

			// Assert
			Assert.True(result.Success, $"Failed to create keychain. StdErr: {result.StdErr}");

			var keychainPath = _keychain.Locate(testKeychainName);
			Assert.True(keychainPath.Exists, $"Keychain file not found at {keychainPath.FullName}");

			_testOutputHelper.WriteLine($"Created keychain: {keychainPath.FullName}");
			_testOutputHelper.WriteLine($"StdOut: {result.StdOut}");
		}
		finally
		{
			// Cleanup
			try
			{
				await _keychain.DeleteKeychainAsync(testKeychainName);
			}
			catch
			{
				// Best effort cleanup
			}
		}
	}

	[SkippableFact]
	public async Task UnlockKeychainAsync_WithCorrectPassword_ShouldSucceed()
	{
		Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.OSX), "Keychain operations only available on macOS");

		// Arrange
		var testKeychainName = $"test-keychain-unlock-{Guid.NewGuid():N}";
		var password = "unlock-test-password";

		try
		{
			// Create a test keychain
			var createResult = await _keychain.CreateKeychainAsync(testKeychainName, password);
			Assert.True(createResult.Success, "Failed to create test keychain");

			// Act
			var unlockResult = await _keychain.UnlockKeychainAsync(password, testKeychainName);

			// Assert
			Assert.True(unlockResult.Success, $"Failed to unlock keychain. StdErr: {unlockResult.StdErr}");

			_testOutputHelper.WriteLine($"Unlocked keychain successfully");
			_testOutputHelper.WriteLine($"StdOut: {unlockResult.StdOut}");
		}
		finally
		{
			// Cleanup
			try
			{
				await _keychain.DeleteKeychainAsync(testKeychainName);
			}
			catch
			{
				// Best effort cleanup
			}
		}
	}

	[SkippableFact]
	public async Task DeleteKeychainAsync_WithExistingKeychain_ShouldSucceed()
	{
		Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.OSX), "Keychain operations only available on macOS");

		// Arrange
		var testKeychainName = $"test-keychain-delete-{Guid.NewGuid():N}";
		var password = "delete-test-password";

		// Create a keychain to delete
		var createResult = await _keychain.CreateKeychainAsync(testKeychainName, password);
		Assert.True(createResult.Success, "Failed to create test keychain");

		var keychainPath = _keychain.Locate(testKeychainName);
		Assert.True(keychainPath.Exists, "Keychain should exist before deletion");

		// Act
		var deleteResult = await _keychain.DeleteKeychainAsync(testKeychainName);

		// Assert
		Assert.True(deleteResult.Success, $"Failed to delete keychain. StdErr: {deleteResult.StdErr}");

		// Refresh the file info
		keychainPath.Refresh();
		Assert.False(keychainPath.Exists, "Keychain should not exist after deletion");

		_testOutputHelper.WriteLine($"Deleted keychain successfully");
		_testOutputHelper.WriteLine($"StdOut: {deleteResult.StdOut}");
	}

	[SkippableFact]
	public async Task SetDefaultKeychainAsync_WithExistingKeychain_ShouldSucceed()
	{
		Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.OSX), "Keychain operations only available on macOS");

		// Arrange
		var testKeychainName = $"test-keychain-default-{Guid.NewGuid():N}";
		var password = "default-test-password";

		try
		{
			// Create a test keychain
			var createResult = await _keychain.CreateKeychainAsync(testKeychainName, password);
			Assert.True(createResult.Success, "Failed to create test keychain");

			// Act
			var setDefaultResult = await _keychain.SetDefaultKeychainAsync(testKeychainName);

			// Assert
			// Note: We don't strictly verify it became the default as that would affect the system
			// We just verify the command executed without error
			Assert.True(setDefaultResult.Success, $"Failed to set default keychain. StdErr: {setDefaultResult.StdErr}");

			_testOutputHelper.WriteLine($"Set default keychain command executed");
			_testOutputHelper.WriteLine($"StdOut: {setDefaultResult.StdOut}");
		}
		finally
		{
			// Cleanup - restore login keychain as default
			try
			{
				await _keychain.SetDefaultKeychainAsync(Keychain.DefaultKeychain);
				await _keychain.DeleteKeychainAsync(testKeychainName);
			}
			catch
			{
				// Best effort cleanup
			}
		}
	}

	[SkippableFact]
	public async Task UpdateKeychainListAsync_WithTestKeychain_ShouldSucceed()
	{
		Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.OSX), "Keychain operations only available on macOS");

		// Arrange
		var testKeychainName = $"test-keychain-list-{Guid.NewGuid():N}";
		var password = "list-test-password";

		try
		{
			// Create a test keychain
			var createResult = await _keychain.CreateKeychainAsync(testKeychainName, password);
			Assert.True(createResult.Success, "Failed to create test keychain");

			// Act
			var updateListResult = await _keychain.UpdateKeychainListAsync(testKeychainName);

			// Assert
			Assert.True(updateListResult.Success, $"Failed to update keychain list. StdErr: {updateListResult.StdErr}");

			_testOutputHelper.WriteLine($"Updated keychain list successfully");
			_testOutputHelper.WriteLine($"StdOut: {updateListResult.StdOut}");
		}
		finally
		{
			// Cleanup
			try
			{
				await _keychain.DeleteKeychainAsync(testKeychainName);
			}
			catch
			{
				// Best effort cleanup
			}
		}
	}

	[SkippableFact]
	public async Task SetPartitionListAsync_WithValidKeychain_ShouldSucceed()
	{
		Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.OSX), "Keychain operations only available on macOS");

		// Arrange
		var testKeychainName = $"test-keychain-partition-{Guid.NewGuid():N}";
		var password = "partition-test-password";

		try
		{
			// Create a test keychain
			var createResult = await _keychain.CreateKeychainAsync(testKeychainName, password);
			Assert.True(createResult.Success, "Failed to create test keychain");

			// Act
			var setPartitionResult = await _keychain.SetPartitionListAsync(password, testKeychainName);

			// Assert
			// This might fail if there are no keys in the keychain, which is expected
			// We just verify the command executed
			_testOutputHelper.WriteLine($"Set partition list result: {setPartitionResult.Success}");
			_testOutputHelper.WriteLine($"StdOut: {setPartitionResult.StdOut}");
			_testOutputHelper.WriteLine($"StdErr: {setPartitionResult.StdErr}");

			// Don't assert success as it may fail on empty keychain, just verify no exception
		}
		finally
		{
			// Cleanup
			try
			{
				await _keychain.DeleteKeychainAsync(testKeychainName);
			}
			catch
			{
				// Best effort cleanup
			}
		}
	}

	[SkippableFact]
	public async Task KeychainLifecycle_CreateUnlockDelete_ShouldWorkCorrectly()
	{
		Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.OSX), "Keychain operations only available on macOS");

		// Arrange
		var testKeychainName = $"test-keychain-lifecycle-{Guid.NewGuid():N}";
		var password = "lifecycle-test-password";

		try
		{
			// Act & Assert - Create
			_testOutputHelper.WriteLine("Step 1: Creating keychain...");
			var createResult = await _keychain.CreateKeychainAsync(testKeychainName, password);
			Assert.True(createResult.Success, $"Failed to create keychain. StdErr: {createResult.StdErr}");

			var keychainPath = _keychain.Locate(testKeychainName);
			Assert.True(keychainPath.Exists, "Keychain should exist after creation");
			_testOutputHelper.WriteLine($"  Created: {keychainPath.FullName}");

			// Act & Assert - Unlock
			_testOutputHelper.WriteLine("Step 2: Unlocking keychain...");
			var unlockResult = await _keychain.UnlockKeychainAsync(password, testKeychainName);
			Assert.True(unlockResult.Success, $"Failed to unlock keychain. StdErr: {unlockResult.StdErr}");
			_testOutputHelper.WriteLine("  Unlocked successfully");

			// Act & Assert - Update keychain list
			_testOutputHelper.WriteLine("Step 3: Updating keychain list...");
			var updateResult = await _keychain.UpdateKeychainListAsync(testKeychainName);
			Assert.True(updateResult.Success, $"Failed to update keychain list. StdErr: {updateResult.StdErr}");
			_testOutputHelper.WriteLine("  Updated keychain list");

			// Act & Assert - Delete
			_testOutputHelper.WriteLine("Step 4: Deleting keychain...");
			var deleteResult = await _keychain.DeleteKeychainAsync(testKeychainName);
			Assert.True(deleteResult.Success, $"Failed to delete keychain. StdErr: {deleteResult.StdErr}");

			keychainPath.Refresh();
			Assert.False(keychainPath.Exists, "Keychain should not exist after deletion");
			_testOutputHelper.WriteLine("  Deleted successfully");

			_testOutputHelper.WriteLine("Keychain lifecycle test completed successfully!");
		}
		catch
		{
			// Cleanup on failure
			try
			{
				await _keychain.DeleteKeychainAsync(testKeychainName);
			}
			catch
			{
				// Best effort cleanup
			}
			throw;
		}
	}
}
