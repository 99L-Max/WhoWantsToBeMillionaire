using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

namespace WhoWantsToBeMillionaire
{
    static class SpritePainter
    {
        public static Image GetSprite(Image sprite, int rowsCount, int columnsCount, int row, int column, bool isDisposeSprite = true)
        {
            var width = sprite.Width / columnsCount;
            var height = sprite.Height / rowsCount;

            var destRect = new Rectangle(0, 0, width, height);
            var srcRect = new Rectangle(column * width, row * height, width, height);

            var result = new Bitmap(destRect.Width, destRect.Height);

            using (var g = Graphics.FromImage(result))
                g.DrawImage(sprite, destRect, srcRect, GraphicsUnit.Pixel);

            if (isDisposeSprite)
                sprite.Dispose();

            return result;
        }

        public static List<Image> GetSpritesList(Image sprite, int rowsCount, int columnsCount, bool isDisposeSprite = true)
        {
            var list = new List<Image>();

            for (int row = 0; row < rowsCount; row++)
                for (int column = 0; column < columnsCount; column++)
                    list.Add(GetSprite(sprite, rowsCount, columnsCount, row, column, false));

            if (isDisposeSprite)
                sprite.Dispose();

            return list;
        }

        public static ReadOnlyDictionary<TKey, Image> GetEnumSpritesList<TKey>(Image sprite) where TKey : Enum
        {
            var keys = CollectionFactory.GetEnum<TKey>();
            var images = GetSpritesList(sprite, keys.Count(), 1);
            var dict = CollectionFactory.JoinToDictionary(keys, images);
            return new ReadOnlyDictionary<TKey, Image>(dict);
        }
    }
}
