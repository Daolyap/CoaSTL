using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CoaSTL.Core.ImageProcessing;

/// <summary>
/// Processes images and converts them to height maps for 3D relief generation.
/// </summary>
public sealed class ImageProcessor : IDisposable
{
    private Image<Rgba32>? _image;
    private bool _disposed;

    /// <summary>
    /// Gets the width of the loaded image.
    /// </summary>
    public int Width => _image?.Width ?? 0;

    /// <summary>
    /// Gets the height of the loaded image.
    /// </summary>
    public int Height => _image?.Height ?? 0;

    /// <summary>
    /// Gets whether an image is currently loaded.
    /// </summary>
    public bool HasImage => _image != null;

    /// <summary>
    /// Loads an image from the specified file path.
    /// </summary>
    public void LoadImage(string filePath)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Image file not found.", filePath);
        }

        _image?.Dispose();
        _image = Image.Load<Rgba32>(filePath);
    }

    /// <summary>
    /// Loads an image from a stream.
    /// </summary>
    public void LoadImage(Stream stream)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _image?.Dispose();
        _image = Image.Load<Rgba32>(stream);
    }

    /// <summary>
    /// Resizes the image to the specified dimensions.
    /// </summary>
    public void Resize(int width, int height)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_image == null)
            throw new InvalidOperationException("No image loaded.");

        _image.Mutate(ctx => ctx.Resize(width, height));
    }

    /// <summary>
    /// Crops the image to the specified rectangle.
    /// </summary>
    public void Crop(int x, int y, int width, int height)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_image == null)
            throw new InvalidOperationException("No image loaded.");

        _image.Mutate(ctx => ctx.Crop(new Rectangle(x, y, width, height)));
    }

    /// <summary>
    /// Rotates the image by the specified degrees.
    /// </summary>
    public void Rotate(float degrees)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_image == null)
            throw new InvalidOperationException("No image loaded.");

        _image.Mutate(ctx => ctx.Rotate(degrees));
    }

    /// <summary>
    /// Converts the image to grayscale.
    /// </summary>
    public void ConvertToGrayscale()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_image == null)
            throw new InvalidOperationException("No image loaded.");

        _image.Mutate(ctx => ctx.Grayscale());
    }

    /// <summary>
    /// Adjusts the brightness of the image.
    /// </summary>
    /// <param name="amount">Brightness adjustment (-1 to 1, 0 = no change)</param>
    public void AdjustBrightness(float amount)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_image == null)
            throw new InvalidOperationException("No image loaded.");

        _image.Mutate(ctx => ctx.Brightness(1 + amount));
    }

    /// <summary>
    /// Adjusts the contrast of the image.
    /// </summary>
    /// <param name="amount">Contrast adjustment (-1 to 1, 0 = no change)</param>
    public void AdjustContrast(float amount)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_image == null)
            throw new InvalidOperationException("No image loaded.");

        _image.Mutate(ctx => ctx.Contrast(1 + amount));
    }

    /// <summary>
    /// Applies binary threshold to convert image to black and white.
    /// </summary>
    /// <param name="threshold">Threshold value (0-1)</param>
    public void ApplyThreshold(float threshold)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_image == null)
            throw new InvalidOperationException("No image loaded.");

        _image.Mutate(ctx => ctx.BinaryThreshold(threshold));
    }

    /// <summary>
    /// Applies Gaussian blur to smooth the image.
    /// </summary>
    /// <param name="sigma">Blur amount</param>
    public void ApplyBlur(float sigma)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_image == null)
            throw new InvalidOperationException("No image loaded.");

        _image.Mutate(ctx => ctx.GaussianBlur(sigma));
    }

    /// <summary>
    /// Inverts the colors of the image.
    /// </summary>
    public void Invert()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_image == null)
            throw new InvalidOperationException("No image loaded.");

        _image.Mutate(ctx => ctx.Invert());
    }

    /// <summary>
    /// Generates a height map from the current image.
    /// Values are normalized to 0-1 range where 0 is black (low) and 1 is white (high).
    /// </summary>
    /// <returns>A 2D array of height values normalized to 0-1.</returns>
    public float[,] GenerateHeightMap()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_image == null)
            throw new InvalidOperationException("No image loaded.");

        var heightMap = new float[_image.Width, _image.Height];

        for (int y = 0; y < _image.Height; y++)
        {
            for (int x = 0; x < _image.Width; x++)
            {
                var pixel = _image[x, y];
                // Convert to grayscale luminance
                var luminance = (0.299f * pixel.R + 0.587f * pixel.G + 0.114f * pixel.B) / 255f;
                // Store with Y-axis flipped for 3D coordinate system
                heightMap[x, _image.Height - 1 - y] = luminance;
            }
        }

        return heightMap;
    }

    /// <summary>
    /// Generates a height map with the specified output dimensions.
    /// </summary>
    public float[,] GenerateHeightMap(int width, int height)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_image == null)
            throw new InvalidOperationException("No image loaded.");

        // Create a resized copy for height map generation
        using var resized = _image.Clone();
        resized.Mutate(ctx => ctx.Resize(width, height));

        var heightMap = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var pixel = resized[x, y];
                var luminance = (0.299f * pixel.R + 0.587f * pixel.G + 0.114f * pixel.B) / 255f;
                heightMap[x, height - 1 - y] = luminance;
            }
        }

        return heightMap;
    }

    /// <summary>
    /// Saves the current image to a file.
    /// </summary>
    public void SaveImage(string filePath)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_image == null)
            throw new InvalidOperationException("No image loaded.");

        _image.Save(filePath);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _image?.Dispose();
            _image = null;
            _disposed = true;
        }
    }
}
