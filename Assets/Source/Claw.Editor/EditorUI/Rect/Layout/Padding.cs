namespace Beast.Claw.Editor
{
    public struct Padding
    {
        public float left;
        public float right;
        public float top;
        public float down;

        public Padding(float left, float right, float top, float down)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.down = down;
        }
        
        public static implicit operator Padding(float padding)
        {
            return new Padding(padding, padding, padding, padding);
        }
        
        public Padding WithLeft(float leftPadding)
        {
            return new Padding(leftPadding, right, top, down);
        }

        public Padding WithRight(float rightPadding)
        {
            return new Padding(left, rightPadding, top, down);
        }

        public Padding WithTop(float topPadding)
        {
            return new Padding(left, right, topPadding, down);
        }

        public Padding WithTopDown(float topDownPadding)
        {
            return new Padding(left, right, topDownPadding, topDownPadding);
        }

        public Padding WithLeftRight(float leftRightPadding)
        {
            return new Padding(leftRightPadding, leftRightPadding, top, down);
        }

        public static Padding TopDown(float topDownPadding)
        {
            return new Padding(0, 0, topDownPadding, topDownPadding);
        }
        
        public static Padding LeftRight(float leftRightPadding)
        {
            return new Padding(leftRightPadding, leftRightPadding, 0, 0);
        }

        public Padding WithDown(float downPadding)
        {
            return new Padding(left, right, top, downPadding);
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

        public static Padding Down(float bottomPadding)
        {
            return new Padding(0, 0, 0, bottomPadding);
        }
    }
}