using System;
using System.IO;
using Unity.Collections;
using UnityEngine;
//using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem;

namespace TexturePainting
{
    public class PaintTest1 : MonoBehaviour
    {
        [SerializeField]
        private MeshRenderer _meshRenderer;
        [SerializeField]
        private int _textureWidth;
        [SerializeField]
        private int _textureHeight;

        private Texture2D _baseMap;
        private NativeArray<Color32> _baseMapPixels;
        private Texture2D _normalMap;
        // TODO: Use struct type that contains 8 bit R, 8 bit G, and 8 bit B.
        private NativeArray<(byte, byte, byte)> _normalMapTexels;

        [SerializeField]
        private Rect _drawingArea;
        [SerializeField]
        private int _innerRadius;
        [SerializeField]
        private int _outerRadius;
        [SerializeField]
        private Color32 _paintColor;

        [Header("Input")]
        [SerializeField]
        private InputAction _cursorPointInputAction;
        [SerializeField]
        private InputAction _cursorClickInputAction;
        //[SerializeField]
        //private Vector2 _cursorPoint;
        //[SerializeField]
        //private Vector2 _drawingPoint;

        [Header("Debug")]
        [SerializeField]
        private InputAction _saveTexturesInputAction;

        private int _prevX;
        private int _prevY;

        private int radius => _innerRadius + _outerRadius;

        private void OnEnable()
        {
            EnableInputActions();

            _baseMap = new Texture2D(_textureWidth, _textureHeight);
            _baseMapPixels = _baseMap.GetPixelData<Color32>(0);
            _meshRenderer.material.SetTexture("_BaseMap", _baseMap);

            //Fill(_baseMapPixels, new Color32(255, 255, 255, 255));
            Fill(_baseMapPixels, new Color32(0, 0, 0, 0));

            //void Fulfills(NativeArray<Color32> pixels, Color32 color)
            //{
            //    for (int i = 0; i < pixels.Length; i++)
            //    {
            //        pixels[i] = color;
            //    }
            //}

            //Fulfills(_baseMapPixels, new Color32(255, 255, 255, 255));

            //NativeArray<Color32> pixelData = _baseMap.GetPixelData<Color32>(0);
            //for (int i = 0; i < pixelData.Length; i++)
            //{
            //    pixelData[i] = new Color32(255, 0, 0, 255);
            //    //pixelData[i] = Color32.Lerp(Color.black, Color.white, (float)i / pixelData.Length);
            //    //Debug.Log($"[{i}] = {pixelData[i]}");
            //}
            //for (int i = 0; i < _baseMap.width; i++)
            //{
            //    for (int j = 0; j < _baseMap.height; j++)
            //    {
            //        pixelData[j * _baseMap.width + i] = new Color32((byte)((float)i / _baseMap.width * 255), (byte)((float)j / _baseMap.height * 255), 0, 255);
            //    }
            //}
            //_baseMap.SetPixelData(pixelData, 0);
            _baseMap.Apply();

            //byte[] png = _baseMap.EncodeToPNG();
            //File.WriteAllBytes($"{Application.dataPath}/test.png", png);

            _normalMap = new Texture2D(_textureWidth, _textureHeight, TextureFormat.RGB24, -1, false);
            _normalMapTexels = _normalMap.GetPixelData<(byte, byte, byte)>(0);
            (byte, byte, byte) rgb = ConvertNormalToRGB(Vector3.forward);
            Fill(_normalMapTexels, rgb);

            _normalMap.Apply();
            _meshRenderer.material.SetTexture("_BumpMap", _normalMap);
            // TODO: Shader variant my be stripped when making a build. Ensure shader variant is included in build.
            _meshRenderer.material.EnableKeyword("_NORMALMAP");

            //byte[] png = _normalMap.EncodeToPNG();
            //File.WriteAllBytes($"{Application.dataPath}/NormalMap.png", png);
        }

        private void OnDisable()
        {
            DisableInputActions();

            if (_baseMap != null)
            {
                Destroy(_baseMap);
                _baseMap = null;
            }
            if (_baseMapPixels.IsCreated)
            {
                _baseMapPixels.Dispose();
            }
        }

        //private void Start()
        //{
        //    _baseMap = new Texture2D(_textureWidth, _textureHeight, DefaultFormat.LDR, TextureCreationFlags.None);

        //    //_meshRenderer.materi;
        //}

        private void Update()
        {
            if (_cursorClickInputAction.WasPressedThisFrame())
            {
                Vector2 cursorPoint = _cursorPointInputAction.ReadValue<Vector2>();
                Vector2 normalizedCursorPoint = Rect.PointToNormalized(_drawingArea, cursorPoint);
                int width = _baseMap.width;
                int height = _baseMap.height;
                (int x, int y) = ConvertNormalizedPointToTexturePosition(width, height, normalizedCursorPoint);

                DrawCircle(_baseMapPixels, _paintColor, width, height, x, y, _outerRadius);
                DrawNormalCircle(_normalMapTexels, _normalMap.width, _normalMap.height, x, y, _innerRadius, _outerRadius);

                _baseMap.Apply();
                _prevX = x;
                _prevY = y;
            }
            else if (_cursorClickInputAction.IsPressed())
            {
                Vector2 cursorPoint = _cursorPointInputAction.ReadValue<Vector2>();
                Vector2 normalizedCursorPoint = Rect.PointToNormalized(_drawingArea, cursorPoint);
                int width = _baseMap.width;
                int height = _baseMap.height;
                (int x, int y) = ConvertNormalizedPointToTexturePosition(width, height, normalizedCursorPoint);

                // TODO: Merge below two methods into one to avoid calling DrawLineBresenham twice.
                DrawLine(_baseMapPixels, _paintColor, width, height, _prevX, _prevY, x, y, _outerRadius);
                //DrawNormalLine(_normalMapTexels, _normalMap.width, _normalMap.height, x, y, _prevX, _prevY, _innerRadius, _outerRadius);
                DrawNormalLine(_normalMapTexels, _normalMap.width, _normalMap.height, _prevX, _prevY, x, y, _innerRadius, _outerRadius);
                //DrawLineBresenham(x, y, _prevX, _prevY, (x, y) =>
                //{
                //    DrawCircle(_baseMapPixels, _paintColor, _baseMap.width, _baseMap.height, x, y, _outerRadius);
                //    // TODO: Draw only half of the circle not to overwrite the previous circle.
                //    DrawNormalCircle(_normalMapTexels, _normalMap.width, _normalMap.height, x, y, _innerRadius, _outerRadius);
                //});

                _baseMap.Apply();
                _normalMap.Apply();
                _prevX = x;
                _prevY = y;
            }
            //else if (_cursorClickInputAction.WasReleasedThisFrame())
            //{
            //    _prevX = default;
            //    _prevY = default;
            //}

            //bool isClicked = _cursorClickInputAction.IsPressed();

            //if (isClicked)
            //{
            //    Vector2 cursorPoint = _cursorPointInputAction.ReadValue<Vector2>();
            //    Vector2 normalizedCursorPoint = Rect.PointToNormalized(_drawingArea, cursorPoint);

            //    DrawCircle(_baseMapPixels, new Color32(0, 0, 0, 255), _baseMap.width, _baseMap.height, normalizedCursorPoint, _outerRadius);
            //    _baseMap.Apply();

            //    DrawNormalCircle(_normalMapTexels, _normalMap.width, _normalMap.height, normalizedCursorPoint, _innerRadius, _outerRadius);
            //    _normalMap.Apply();
            //}

            if (_saveTexturesInputAction.WasPressedThisFrame())
            {
                byte[] basemapPng = _baseMap.EncodeToPNG();
                File.WriteAllBytes($"{Application.dataPath}/BaseMap.png", basemapPng);
                byte[] normalMapPng = _normalMap.EncodeToPNG();
                File.WriteAllBytes($"{Application.dataPath}/NormalMap.png", normalMapPng);
            }
        }

        private void EnableInputActions()
        {
            _cursorPointInputAction.Enable();
            //_cursorPointInputAction.started += CursorPointInputActionCallback;
            //_cursorPointInputAction.performed += CursorPointInputActionCallback;
            //_cursorPointInputAction.canceled += CursorPointInputActionCallback;

            _cursorClickInputAction.Enable();

            _saveTexturesInputAction.Enable();
        }

        private void DisableInputActions()
        {
            //_cursorPointInputAction.started -= CursorPointInputActionCallback;
            //_cursorPointInputAction.performed -= CursorPointInputActionCallback;
            //_cursorPointInputAction.canceled -= CursorPointInputActionCallback;
            _cursorPointInputAction.Disable();

            _cursorClickInputAction.Disable();

            _saveTexturesInputAction.Disable();
        }

        //private void CursorPointInputActionCallback(InputAction.CallbackContext context)
        //{
        //    _cursorPoint = context.ReadValue<Vector2>();
        //    _drawingPoint = Rect.PointToNormalized(_drawingArea, _cursorPoint);
        //}

        private static (int x, int y) ConvertNormalizedPointToTexturePosition(int width, int height, Vector2 normalizedPoint)
        {
            int x = Mathf.Min((int)(normalizedPoint.x * width), width - 1);
            int y = Mathf.Min((int)(normalizedPoint.y * height), height - 1);
            return (x, y);
        }

        private static bool IsInRect(int x, int y, int width, int height)
        {
            return (0 <= x && x < width) && (0 <= y && y < height);
        }

        private static void PlotIfInRect<T>(NativeArray<T> pixles, T color, int width, int height, int x, int y) where T : struct
        {
            if (IsInRect(x, y, width, height))
            {
                pixles[x + y * height] = color;
            }
        }

        private static void Fill<T>(NativeArray<T> array, T data) where T : struct
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = data;
            }
        }

        private static void DrawCircle<T>(NativeArray<T> pixels, T color, int width, int height, Vector2 normalizedPoint, int radius) where T : struct
        {
            //int x = Mathf.Min((int)(normalizedPoint.x * width), width - 1);
            //int y = Mathf.Min((int)(normalizedPoint.y * height), height - 1);

            //for (int i = -radius; i <= radius; i++)
            //{
            //    for (int j = -radius; j <= radius; j++)
            //    {
            //        if ((0 <= x + i && x + i < width) && (0 <= j + y && j + y < height))
            //        {
            //            if (i * i + j * j <= radius * radius)
            //            {
            //                pixels[(x + i) + (y + j) * height] = color;
            //            }
            //        }
            //    }
            //}

            (int x, int y) = ConvertNormalizedPointToTexturePosition(width, height, normalizedPoint);
            DrawCircle(pixels, color, width, height, x, y, radius);
        }

        private static void DrawCircle<T>(NativeArray<T> pixles, T color, int width, int height, int x, int y, int radius) where T : struct
        {
            for (int i = 0; i < radius; i++)
            {
                for (int j = 0; j < radius; j++)
                {
                    if (i * i + j * j <= radius * radius)
                    {
                        PlotIfInRect(pixles, color, width, height, x + i, y + j);
                        PlotIfInRect(pixles, color, width, height, x - i, y + j);
                        PlotIfInRect(pixles, color, width, height, x - i, y - j);
                        PlotIfInRect(pixles, color, width, height, x + i, y - j);
                    }
                }
            }
        }

        private static void DrawLine<T>(NativeArray<T> pixels, T color, int width, int height, int x1, int y1, int x2, int y2, int thickness) where T : struct
        {
            DrawLineBresenham(x1, y1, x2, y2, (x, y) =>
            {
                DrawCircle(pixels, color, width, height, x, y, thickness);
            });
        }

        private static (byte r, byte g, byte b) ConvertNormalToRGB(Vector3 normal)
        {
            //normal.Normalize();
            normal += Vector3.one;
            normal /= 2f;
            return ((byte)(normal.x * 255), (byte)(normal.y * 255), (byte)(normal.z * 255));
        }

        private Vector3 ConvertRGBToNormal(byte r, byte g, byte b)
        {
            Vector3 normal = new Vector3(r, g, b) * 2;
            normal -= Vector3.one;
            return normal;
        }

        private static void DrawNormalCircle(NativeArray<(byte r, byte g, byte b)> texels, int width, int height, int x, int y, int innerRadius, int outerRadius)
        {
            void PlotIfConditionMet(NativeArray<(byte r, byte g, byte b)> texels, (byte r, byte g, byte b) texel, int width, int height, int x, int y)
            {
                if (IsInRect(x, y, width, height))
                {
                    //if (texels[x + y * height].b < texel.b)
                    {
                        texels[x + y * height] = texel;
                    }
                }
            }

            for (int i = 0; i < outerRadius; i++)
            {
                for (int j = 0; j < outerRadius; j++)
                {
                    float scale = (float)(i * i + j * j - innerRadius * innerRadius) / (outerRadius * outerRadius - innerRadius * innerRadius);
                    if (scale < 1)
                    {
                        // TODO: Adjust Z value.
                        float length = new Vector3(i * scale, j * scale, outerRadius - innerRadius).magnitude;
                        PlotIfConditionMet(texels, ConvertNormalToRGB(new Vector3(i * scale, j * scale, outerRadius - innerRadius) / length), width, height, x + i, y + j);
                        PlotIfConditionMet(texels, ConvertNormalToRGB(new Vector3(-i * scale, j * scale, outerRadius - innerRadius) / length), width, height, x - i, y + j);
                        PlotIfConditionMet(texels, ConvertNormalToRGB(new Vector3(-i * scale, -j * scale, outerRadius - innerRadius) / length), width, height, x - i, y - j);
                        PlotIfConditionMet(texels, ConvertNormalToRGB(new Vector3(i * scale, -j * scale, outerRadius - innerRadius) / length), width, height, x + i, y - j);
                        //PlotIfInRect(texels, ConvertNormalToRGB(new Vector3(i * scale, j * scale, outerRadius - innerRadius) / length), width, height, x + i, y + j);
                        //PlotIfInRect(texels, ConvertNormalToRGB(new Vector3(-i * scale, j * scale, outerRadius - innerRadius) / length), width, height, x - i, y + j);
                        //PlotIfInRect(texels, ConvertNormalToRGB(new Vector3(-i * scale, -j * scale, outerRadius - innerRadius) / length), width, height, x - i, y - j);
                        //PlotIfInRect(texels, ConvertNormalToRGB(new Vector3(i * scale, -j * scale, outerRadius - innerRadius) / length), width, height, x + i, y - j);
                    }
                }
            }

            //for (int i = -outerRadius; i <= outerRadius; i++)
            //{
            //    if (x + i < 0 || width < x + i)
            //    {
            //        continue;
            //    }

            //    for (int j = -outerRadius; j <= outerRadius; j++)
            //    {
            //        if (y + j < 0 || height < y + j)
            //        {
            //            continue;
            //        }

            //        float scale = (float)(i * i + j * j - innerRadius * innerRadius) / (outerRadius * outerRadius - innerRadius * innerRadius);
            //        if (scale < 1)
            //        {
            //            // TODO: Adjust Z value.
            //            Vector3 normal = new Vector3(i * scale, j * scale, outerRadius - innerRadius).normalized;
            //            texels[(x + i) + (y + j) * height] = ConvertNormalToRGB(normal);
            //            //Debug.Log($"Center = ({x:00},{y:00}): Loop = ({i:00},{j:00}): ({x + i:00},{y + j:00}) = {normal:F4}, Scale = {scale}");
            //        }
            //    }
            //}
        }

        private static void DrawNormalHalfCircle(NativeArray<(byte r, byte g, byte b)> texels, int width, int height, int x, int y, int innerRadius, int outerRadius, Vector2 forward)
        {
            for (int i = 0; i < outerRadius; i++)
            {
                for (int j = 0; j < outerRadius; j++)
                {
                    float scale = (float)(i * i + j * j - innerRadius * innerRadius) / (outerRadius * outerRadius - innerRadius * innerRadius);
                    if (scale < 1)
                    {
                        // TODO: Adjust Z value.
                        float length = new Vector3(i * scale, j * scale, outerRadius - innerRadius).magnitude;
                        if (Vector2.Dot(forward, new Vector2(i, j)) > 0)
                            PlotIfInRect(texels, ConvertNormalToRGB(new Vector3(i * scale, j * scale, outerRadius - innerRadius) / length), width, height, x + i, y + j);
                        if (Vector2.Dot(forward, new Vector2(-i, j)) > 0)
                            PlotIfInRect(texels, ConvertNormalToRGB(new Vector3(-i * scale, j * scale, outerRadius - innerRadius) / length), width, height, x - i, y + j);
                        if (Vector2.Dot(forward, new Vector2(-i, -j)) > 0)
                            PlotIfInRect(texels, ConvertNormalToRGB(new Vector3(-i * scale, -j * scale, outerRadius - innerRadius) / length), width, height, x - i, y - j);
                        if (Vector2.Dot(forward, new Vector2(i, -j)) > 0)
                            PlotIfInRect(texels, ConvertNormalToRGB(new Vector3(i * scale, -j * scale, outerRadius - innerRadius) / length), width, height, x + i, y - j);

                        //float length2D = Mathf.Sqrt(i * i + j * j);
                        //if (Vector2.Dot(forward, new Vector2(i, j) / length2D) > 0.5f)
                        //    PlotIfInRect(texels, ConvertNormalToRGB(new Vector3(i * scale, j * scale, outerRadius - innerRadius) / length), width, height, x + i, y + j);
                        //if (Vector2.Dot(forward, new Vector2(-i, j) / length2D) > 0.5f)
                        //    PlotIfInRect(texels, ConvertNormalToRGB(new Vector3(-i * scale, j * scale, outerRadius - innerRadius) / length), width, height, x - i, y + j);
                        //if (Vector2.Dot(forward, new Vector2(-i, -j) / length2D) > 0.5f)
                        //    PlotIfInRect(texels, ConvertNormalToRGB(new Vector3(-i * scale, -j * scale, outerRadius - innerRadius) / length), width, height, x - i, y - j);
                        //if (Vector2.Dot(forward, new Vector2(i, -j) / length2D) > 0.5f)
                        //    PlotIfInRect(texels, ConvertNormalToRGB(new Vector3(i * scale, -j * scale, outerRadius - innerRadius) / length), width, height, x + i, y - j);
                    }
                }
            }
        }

        private static void DrawNormalLine(NativeArray<(byte r, byte g, byte b)> texels, int width, int height, int x1, int y1, int x2, int y2, int innerRadius, int outerRadius)
        {
            //DrawNormalCircle(texels, width, height, x1, y1, innerRadius, outerRadius);
            Vector2 forward = new Vector2(x2 - x1, y2 - y1).normalized;

            DrawLineBresenham(x1, y1, x2, y2, (x, y) =>
            {
                DrawNormalHalfCircle(texels, width, height, x, y, innerRadius, outerRadius, forward);
                //DrawNormalCircle(texels, width, height, x, y, innerRadius, outerRadius);
            });
        }

        private void DrawNormalCircle(NativeArray<(byte r, byte g, byte b)> texels, int width, int height, Vector2 normalizedPoint, int innerRadius, int outerRadius)
        {
            int x = Mathf.Min((int)(normalizedPoint.x * width), width - 1);
            int y = Mathf.Min((int)(normalizedPoint.y * height), height - 1);

            for (int i = -outerRadius; i <= outerRadius; i++)
            {
                if (x + i < 0 || width < x + i)
                {
                    continue;
                }

                for (int j = -outerRadius; j <= outerRadius; j++)
                {
                    if (y + j < 0 || height < y + j)
                    {
                        continue;
                    }

                    float scale = (float)(i * i + j * j - innerRadius * innerRadius) / (outerRadius * outerRadius - innerRadius * innerRadius);
                    if (scale < 1)
                    {
                        // TODO: Adjust Z value.
                        Vector3 normal = new Vector3(i * scale, j * scale, outerRadius - innerRadius).normalized;
                        texels[(x + i) + (y + j) * height] = ConvertNormalToRGB(normal);
                        //Debug.Log($"Center = ({x:00},{y:00}): Loop = ({i:00},{j:00}): ({x + i:00},{y + j:00}) = {normal:F4}, Scale = {scale}");
                    }

                    //if ((0 <= x + i && x + i < width) && (0 <= j + y && j + y < height))
                    //{
                    //    if (i * i + j * j <= innerRadius * innerRadius)
                    //    {
                    //        texels[(x + i) + (y + j) * height] = ConvertNormalToRGB(Vector3.forward);
                    //    }
                    //    else if (i * i + j * j <= outerRadius * outerRadius)
                    //    {
                    //        texels[(x + i) + (y + j) * height] = ConvertNormalToRGB((new Vector3(i, j, innerRadius)).normalized);
                    //    }
                    //}
                }
            }
        }

        //public static void FillColor32(this Texture2D tex, Color32 color)
        //{
        //    Color32[] pixels = tex.GetPixels32();
        //    for (var i = 0; i < pixels.Length; i++)
        //    {
        //        pixels[i] = color;
        //    }
        //    tex.SetPixels32(pixels);
        //}

        //public static void DrawCircle(this Texture2D tex, int centerX, int centerY, int radius, Color color)
        //{
        //    DrawCircle(centerX, centerY, radius, (x, y) => {
        //        if (x >= 0 && tex.width > x && y >= 0 && tex.height > y)
        //        {
        //            tex.SetPixel(x, y, color);
        //        }
        //    });
        //}

        //public static void DrawCircle(int centerX, int centerY, int radius, Action<int, int> plotPixel)
        //{
        //    for (int x = centerX - radius / 2; x < centerX + radius / 2; x++)
        //    {
        //        for (int y = centerY - radius / 2; y < centerY + radius / 2; y++)
        //        {
        //            if ((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY) < (radius / 2 + 1) * (radius / 2 + 1))
        //            {
        //                plotPixel?.Invoke(x, y);
        //            }
        //        }
        //    }
        //}

        //public static void DrawLineBresenham(this Texture2D tex, int x1, int y1, int x2, int y2, int thickness, Color color)
        //{
        //    DrawLineBresenham(x1, y1, x2, y2, thickness, (x, y) => {
        //        if (x >= 0 && tex.width > x && y >= 0 && tex.height > y)
        //        {
        //            tex.SetPixel(x, y, color);
        //        }
        //    });
        //}

        public static void DrawLineBresenham(int x1, int y1, int x2, int y2, Action<int, int> plotPixel)
        {
            int x = x1;
            int y = y1;
            int ix = x2 - x1 > 0 ? 1 : -1;
            int iy = y2 - y1 > 0 ? 1 : -1;

            int dx = x2 - x1;
            int dy = y2 - y1;
            dx = dx > 0 ? dx : -dx;
            dy = dy > 0 ? dy : -dy;

            int e = dx > dy ? -dx : -dy;
            int d = dx > dy ? dx : dy;
            int ie1 = dx > dy ? 2 * dy : 2 * dx;
            int ie2 = dx > dy ? -2 * dx : -2 * dy;
            int ix1 = dx > dy ? ix : 0;
            int ix2 = dx > dy ? 0 : ix;
            int iy1 = dx > dy ? 0 : iy;
            int iy2 = dx > dy ? iy : 0;

            while (d > 0)
            {
                plotPixel?.Invoke(x, y);

                e += ie1;
                x += ix1;
                y += iy1;
                if (e >= 0)
                {
                    x += ix2;
                    y += iy2;
                    e += ie2;
                }

                d -= 1;
            }
        }

        //public static void DrawLineBresenham(int x1, int y1, int x2, int y2, int thickness, Action<int, int> plotPixel)
        //{
        //    int x = x1;
        //    int y = y1;
        //    int ix = x2 - x1 > 0 ? 1 : -1;
        //    int iy = y2 - y1 > 0 ? 1 : -1;

        //    int dx = x2 - x1;
        //    int dy = y2 - y1;
        //    dx = dx > 0 ? dx : -dx;
        //    dy = dy > 0 ? dy : -dy;

        //    int e = dx > dy ? -dx : -dy;
        //    int d = dx > dy ? dx : dy;
        //    int ie1 = dx > dy ? 2 * dy : 2 * dx;
        //    int ie2 = dx > dy ? -2 * dx : -2 * dy;
        //    int ix1 = dx > dy ? ix : 0;
        //    int ix2 = dx > dy ? 0 : ix;
        //    int iy1 = dx > dy ? 0 : iy;
        //    int iy2 = dx > dy ? iy : 0;

        //    while (d > 0)
        //    {

        //        int dd = 0;
        //        int ixx = dx > dy ? 0 : 1;
        //        int iyy = dx > dy ? 1 : 0;
        //        int xx = dx > dy ? x : x - thickness / 2;
        //        int yy = dx > dy ? y - thickness / 2 : y;

        //        while (dd < thickness)
        //        {
        //            plotPixel?.Invoke(xx, yy);
        //            xx += ixx;
        //            yy += iyy;
        //            dd += 1;
        //        }

        //        e += ie1;
        //        x += ix1;
        //        y += iy1;
        //        if (e >= 0)
        //        {
        //            x += ix2;
        //            y += iy2;
        //            e += ie2;
        //        }

        //        d -= 1;
        //    }
        //    DrawCircle(x1, y1, thickness, plotPixel);
        //    DrawCircle(x2, y2, thickness, plotPixel);
        //}
    }
}
