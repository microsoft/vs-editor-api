//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

using System;

namespace Microsoft.VisualStudio.Core.Imaging
{
    public readonly struct ImageDescription : IEquatable<ImageDescription>
    {
        public readonly ImageId Id;
        public readonly int Width;
        public readonly int Height;
        public readonly ImageTags Tags;

        public ImageDescription(ImageId id, ImageTags tags)
             : this(id, 0, 0, tags)
        {
        }

        public ImageDescription(ImageId id, int size)
             : this(id, size, size, ImageTags.None)
        {
        }

        public ImageDescription(ImageId id, int size, ImageTags tags)
             : this(id, size, size, tags)
        {
        }

        public ImageDescription(ImageId id, int width, int height, ImageTags tags)
        {
            Id = id;
            Width = width;
            Height = height;
            Tags = tags;
        }

        public ImageDescription WithAdditionalTags(ImageTags tags)
            => new ImageDescription(Id, Width, Height, Tags | tags);

        public ImageDescription WithoutTags(ImageTags tags)
            => new ImageDescription(Id, Width, Height, Tags & ~tags);

        public override string ToString ()
            => $"'{Id}' @ {Width}x{Height} [{Tags}]";

        public bool Equals(ImageDescription other)
            => Width == other.Width &&
                Height == other.Height &&
                Tags == other.Tags &&
                Id.Equals(other.Id);

        public override bool Equals(object obj)
            => obj is ImageDescription other && Equals(other);

        public override int GetHashCode()
        {
            var hash = 23;
            hash = hash * 31 + Id.GetHashCode();
            hash = hash * 31 + (int)Tags;
            hash = hash * 31 + Width;
            hash = hash * 31 + Height;
            return hash;
        }

        public static bool operator ==(ImageDescription left, ImageDescription right)
            => left.Equals(right);

        public static bool operator !=(ImageDescription left, ImageDescription right)
            => !(left == right);
    }
}
