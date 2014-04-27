using System;

namespace calibrateServoAngle

{
    class Line
    {
        private double length;   // Length of a line
        public Line()
        {
            //Console.WriteLine("Object is being created");
        }
        public Line(double a)
        {
            setLength(a);
        }

        public void setLength(double len)
        {
            length = len;
        }
        public double getLength()
        {
            return length;
        }

    }
}