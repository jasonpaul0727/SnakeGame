// Author: Daniel Kopta, Fall 2017
// Staff solution for CS 3500 SpaceWars project
// University of Utah

using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace SnakeGame
{
    /// <summary>
    /// A class to represent a 2D Vector in space
    /// </summary>
    [DataContract(Name = "SnakeGame", Namespace = "")]
    public class Vector2D
        // data contract
    {
        [JsonProperty(PropertyName = "X")]
        [DataMember]
        public double x { get; set; }
        [JsonProperty(PropertyName = "Y")]
        [DataMember]
        public double y { get; set; }

        /// <summary>
        /// Default constructor, needed for JSON serialize/deserialize
        /// </summary>
        public Vector2D()
        {
            x = -1;
            y = -1;
        }

        /// <summary>
        /// Two param constructor for x and y.
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        public Vector2D(double _x, double _y)
        {
            x = _x;
            y = _y;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="other"></param>
        public Vector2D(Vector2D other)
        {
            x = other.x;
            y = other.y;
        }

        /// <summary>
        /// Determine if this vector is equal to another
        /// </summary>
        /// <param name="obj">The other vector</param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            // If parameter cannot be cast to Vector return false.
            if (obj is not Vector2D p)
            {
                return false;
            }

            return ToString() == p.ToString();
        }

        /// <summary>
        /// Determine the hashcode for this vector
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// Return a string representation of this vector for debug printing
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "(" + x + "," + y + ")";
        }

        /// <summary>
        /// Get the x component
        /// </summary>
        /// <returns></returns>
        public double GetX()
        {
            return x;
        }

        /// <summary>
        /// Get the y component
        /// </summary>
        /// <returns></returns>
        public double GetY()
        {
            return y;
        }

        /// <summary>
        /// Clamp x and y to be within the range -1 .. 1
        /// </summary>
        public void Clamp()
        {
            if (x > 1.0)
                x = 1.0;
            if (x < -1.0)
                x = -1.0;
            if (y > 1.0)
                y = 1.0;
            if (y < -1.0)
                y = -1.0;
        }

        /// <summary>
        /// Rotate this vector clockwise by degrees
        /// Requires that this vector be normalized
        /// </summary>
        /// <param name="degrees"></param>
        public void Rotate(double degrees)
        {
            double radians = (degrees / 180) * Math.PI;

            double newX = x * Math.Cos(radians) - y * Math.Sin(radians);
            double newY = x * Math.Sin(radians) + y * Math.Cos(radians);

            x = newX;
            y = newY;

            // sin and cos can return numbers outside the valid range due to floating point imprecision,
            // and poor design of C#'s math library
            Clamp();
        }

        /// <summary>
        /// Return the angle as measured in degrees clockwise from up
        /// Requires that this vector be normalized
        /// </summary>
        /// <returns></returns>
        public float ToAngle()
        {
            float theta = (float)Math.Acos(-y);

            if (x < 0.0)
                theta *= -1.0f;

            // Convert to degrees
            theta *= (180.0f / (float)Math.PI);

            return theta;
        }

        /// <summary>
        /// Compute the clockwise angle of the vector pointing from b to a
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float AngleBetweenPoints(Vector2D a, Vector2D b)
        {
            Vector2D bToA = a - b;
            bToA.Normalize();
            return bToA.ToAngle();
        }

        /// <summary>
        /// Add two vectors with the + operator
        /// </summary>
        /// <param name="v1">The left hand side</param>
        /// <param name="v2">The right hand side</param>
        /// <returns></returns>
        public static Vector2D operator +(Vector2D v1, Vector2D v2)
        {
            return new Vector2D(v1.x + v2.x, v1.y + v2.y);
        }

        /// <summary>
        /// Subtract two vectors with the - operator
        /// </summary>
        /// <param name="v1">The left hand side</param>
        /// <param name="v2">The right hand side</param>
        /// <returns></returns>
        public static Vector2D operator -(Vector2D v1, Vector2D v2)
        {
            return new Vector2D(v1.x - v2.x, v1.y - v2.y);
        }

        /// <summary>
        /// Multiply a vector by a scalar
        /// This has the effect of growing (if s greater than 1) or shrinking (if s less than 1),
        /// without changing the direction.
        /// </summary>
        /// <param name="v">The vector (left-hand side of the operator)</param>
        /// <param name="s">The scalar (right-hand side of the operator)</param>
        /// <returns></returns>
        public static Vector2D operator *(Vector2D v, double s)
        {
            Vector2D retval = new Vector2D();
            retval.x = v.GetX() * s;
            retval.y = v.GetY() * s;
            return retval;
        }

        /// <summary>
        /// Compute the length of this vector
        /// </summary>
        /// <returns></returns>
        public double Length()
        {
            return Math.Sqrt(x * x + y * y);
        }

        /// <summary>
        /// Set this vector's length to 1 without changing its direction
        /// </summary>
        public void Normalize()
        {
            double len = Length();
            x /= len;
            y /= len;
        }

        /// <summary>
        /// Compute the dot product of this vector with another vector
        /// </summary>
        /// <param name="v">The other vector</param>
        /// <returns></returns>
        public double Dot(Vector2D v)
        {
            return GetX() * v.GetX() + GetY() * v.GetY();
        }

        /// <summary>
        /// Determines if this cardinal direction is the opposite of the specified cardinal direction.
        /// Both this and other must be normalized cardinal directions.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsOppositeCardinalDirection(Vector2D other)
        {
            return (x == 0 && other.x == 0 && y == -other.y) || (y == 0 && other.y == 0 && x == -other.x);
        }

    }
}