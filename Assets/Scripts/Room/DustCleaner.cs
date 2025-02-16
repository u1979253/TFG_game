using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ggj25
{
    public class DustCleaner
    {
        private const string DUST_LAYER = "Dust";

        private SpriteRenderer _dust;

        private HeroController _hero;

        private Vector3 _previousPosition;
        private Color32[] _currentColor;

        private Transform pos;

        private Rect _dustRect;
        private Texture2D _texture;
        private List<Color> _colorsClean;
        private readonly int _size;

        public DustCleaner(SpriteRenderer dust)
        {
            _dust = dust;

            var dustSprite = CloneSprite(_dust.sprite);
            _dust.sprite = dustSprite;

            _hero = GameObject.FindObjectOfType<HeroController>();
            _previousPosition = _hero.transform.position;
            _size = Mathf.FloorToInt(_hero.Config.CleanSize /Mathf.Max(_dust.transform.lossyScale.x, _dust.transform.lossyScale.y));
            
            CalculateDustRect();
            InitCleanColors();
        }

        private void InitCleanColors()
        {
            int sizeX = _size;
            int sizeY = _size;
            Color colorClear = Color.clear;
            _colorsClean = new List<Color>(sizeX * sizeY);
            for (int i = 0; i < sizeX * sizeY; i++)
            {
                _colorsClean.Add(colorClear);
            }
        }

        private void CalculateDustRect()
        {
            var dustWidth = _dust.sprite.textureRect.width / _dust.sprite.pixelsPerUnit; 
            dustWidth *= _dust.transform.lossyScale.x;
            var dustHeight = _dust.sprite.textureRect.height / _dust.sprite.pixelsPerUnit;
            dustHeight *= _dust.transform.lossyScale.y;

            var dustX = _dust.transform.position.x - dustWidth *0.5f;
            var dustY = _dust.transform.position.y -dustHeight*0.5f;
            
            _dustRect = new Rect(dustX, dustY, dustWidth, dustHeight);
        }

        public void CleanPlayer(Vector3 playerPosition)
        {
            PaintPosition(playerPosition, _size, _colorsClean);
        }

        public void PaintOther(Vector3 position, int cleanSize, Color colorToPaint)
        {
            List<Color> color = new List<Color>();
            for (int i = 0; i < cleanSize*cleanSize; i++)
            {
                color.Add(colorToPaint);
            }
            PaintPosition(position, cleanSize, color);
        }

        private void PaintPosition(Vector3 position, int size, List<Color> color)
        {
            var percentageX = (position.x - _dustRect.xMin) / (_dustRect.xMax - _dustRect.xMin);
            var percentageY = (position.y - _dustRect.yMin) / (_dustRect.yMax - _dustRect.yMin);

            var pixelX = Mathf.RoundToInt(percentageX * _dust.sprite.textureRect.width);
            var pixelY = Mathf.RoundToInt(percentageY * _dust.sprite.textureRect.height);

            int sizeX = size;
            int sizeY = size;

            pixelX -= Mathf.FloorToInt(size * 0.5f);
            pixelY -= Mathf.FloorToInt(size * 0.5f);

            var colors = color;

            /*
            if (pixelX < 0
                && pixelX + _dust.sprite.textureRect.width - Mathf.FloorToInt(size) < 0
                || pixelX > _dust.sprite.textureRect.width - Mathf.FloorToInt(size) 
                && pixelX + _dust.sprite.textureRect.width - Mathf.FloorToInt(size) > _dust.sprite.textureRect.width - Mathf.FloorToInt(size))
            {
                return;
            }
            
            if (pixelY < 0
                && pixelY + _dust.sprite.textureRect.height - Mathf.FloorToInt(size) < 0
                || pixelY > _dust.sprite.textureRect.height - Mathf.FloorToInt(size) 
                && pixelY + _dust.sprite.textureRect.height - Mathf.FloorToInt(size) > _dust.sprite.textureRect.height - Mathf.FloorToInt(size))
            {
                return;
            }
            /*/

            pixelX = Mathf.Clamp(pixelX, 0,
                                 (int)_dust.sprite.textureRect.width - Mathf.FloorToInt(size));
            pixelY = Mathf.Clamp(pixelY, 0,
                                 (int)_dust.sprite.textureRect.height - Mathf.FloorToInt(size));
            /* */
            _dust.sprite.texture.SetPixels(
                pixelX,
                pixelY,
                size,
                size,
                colors.ToArray());
            _dust.sprite.texture.Apply();
        }

        private Sprite CloneSprite(Sprite source)
        {
// Create a copy of the texture by reading and applying the raw texture data.
            Texture2D textureCloned = CloneTexture(source.texture);
            return Sprite.Create(textureCloned, new Rect(0,0, textureCloned.width, textureCloned.height), new Vector2(0.5f, 0.5f), source.pixelsPerUnit);
        }

        private Texture2D CloneTexture(Texture2D source)
        {
// Create a copy of the texture by reading and applying the raw texture data.
            Texture2D texCopy = new Texture2D(source.width, source.height, source.format, source.mipmapCount > 1);
            var newData = _dust.sprite.texture.GetRawTextureData();
// Load the original texture data
            texCopy.LoadRawTextureData(newData);
            texCopy.Apply();
            return texCopy;
        }
    }
}
