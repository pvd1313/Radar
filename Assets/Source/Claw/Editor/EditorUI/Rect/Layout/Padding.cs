namespace Beast.Claw.Editor
{
    public struct Padding
    {
        public float left;
        public float right;
        public float top;
        public float bottom;

        public Padding(float left, float right, float top, float bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }
        
        public Padding WithLeft(float leftPadding)
        {
            return new Padding(leftPadding, right, top, bottom);
        }

        public Padding WithRight(float rightPadding)
        {
            return new Padding(left, rightPadding, top, bottom);
        }

        public Padding WithTop(float topPadding)
        {
            return new Padding(left, right, topPadding, bottom);
        }

        public Padding WithBottom(float bottomPadding)
        {
            return new Padding(left, right, top, bottomPadding);
        }

        public static Padding Left(float leftPadding)
        {
            return new Padding(leftPadding, 0, 0, 0);
        }

        public static Padding Right(float rightPadding)
        {
            return new Padding(0, rightPadding, 0, 0);
        }

        public static Padding Top(float topPadding)
        {
            return new Padding(0, 0, topPadding, 0);
        }

        public static Padding Bottom(float bottomPadding)
        {
            return new Padding(0, 0, 0, bottomPadding);
        }
        
        public static implicit operator Padding(float padding)
        {
            return new Padding(padding, padding, padding, padding);
        }
    }
}