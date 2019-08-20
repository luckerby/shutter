using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karatsuba
{
    public class Number
    {
        public bool isPositive = true;
        public byte[] digits;

        public void BuildNumberFromString(string numberAsString)
        {
            digits = new byte[numberAsString.Length];

            // Convert the strings to a sequence of digits, for easier
            //  processing
            for (int i = 0; i < numberAsString.Length; i++)
            {
                digits[i] = (byte)(numberAsString[i] - 48);
            }
        }

        // We need this to simplify unit tests
        public override bool Equals(object obj)
        {
            // Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Number target = (Number)obj;
                if(isPositive==target.isPositive &&
                    digits.SequenceEqual(target.digits))
                        return true;
            }
            return false;
        }
    }

    public class Program
    {
        static public bool isSubstractionSignPlus;

        static void Main(string[] args)
        {
            // The input numbers, stored as strings so there's no real
            //  limit on their length. Assume the length to be a power of 2
            string xAsString = "3141592653589793238462643383279502884197169399375105820974944592";
            string yAsString = "2718281828459045235360287471352662497757247093699959574966967627";

            Number x = new Number();
            x.BuildNumberFromString(xAsString);
            x.isPositive = true;
            Number y = new Number();
            y.BuildNumberFromString(yAsString);
            y.isPositive = false;

            //Sum(x, y);

            //UpLengthOfNumberToClosestPowerOfTwo(ref x, ref y);

            //TrimNumber(ref x);

            Product(x, y);
            
        }

        static public Number Sum(Number x, Number y)
        {
            // Pad the number whose length is smaller first
            PadNumber(ref x, ref y);
            
            Number sum = new Number();
            if (x.isPositive && y.isPositive)
            {
                // Regular sum
                sum.digits = DigitSum(x.digits, y.digits);
            }
            else if (!x.isPositive && !y.isPositive)
            {
                // Still a regular sum, but the sign of the sum will be minus
                sum.digits = DigitSum(x.digits, y.digits);
                sum.isPositive = false;
            }
            else if (x.isPositive && !y.isPositive)
            {
                // Substract y from x
                sum.digits = DigitSubstract(x.digits, y.digits);
                // Use the global flag to determine the result's sign
                sum.isPositive = isSubstractionSignPlus;
            }
            else if (!x.isPositive && y.isPositive)
            {
                // Substract x from y
                sum.digits = DigitSubstract(y.digits, x.digits);
                // Use the global flag to determine the result's sign
                sum.isPositive = isSubstractionSignPlus;
            }

            return sum;
        }

        static public Number Product(Number x, Number y)
        {
            // !! Check here that the input is always 2 equally long
            //  numbers, even when recursing !!

            Number product = new Number();
            if (x.digits.Length == 1 && y.digits.Length == 1)
            {
                // !Leave the sign alone for now
                //product.isPositive
                // maximum 81 will be returned
                //product.digits = new byte[] { (byte)(x.digits[0] * y.digits[0]) };

                product.digits = new byte[] { 0 };
                for (int count=0;count<y.digits[0];count++)
                {
                    product = Sum(product, x);
                }
            }   
            else
            {
                // Pad both numbers so that they have a length of power of 2
                UpLengthOfNumberToClosestPowerOfTwo(ref x, ref y);

                // Since both numbers have been previously "upped" to a common
                //  length, 'numberLength' will contain their common length
                byte numberLength = (byte)x.digits.Length;

                Number a = new Number();
                a.digits = new byte[numberLength / 2];
                Number b = new Number();
                b.digits = new byte[numberLength / 2];
                Number c = new Number();
                c.digits = new byte[numberLength / 2];
                Number d = new Number();
                d.digits = new byte[numberLength / 2];

                // Build a, b, c and d
                for (int i = 0; i < numberLength / 2; i++)
                {
                    a.digits[i] = x.digits[i];
                    b.digits[i] = x.digits[i + numberLength / 2];
                    c.digits[i] = y.digits[i];
                    d.digits[i] = y.digits[i + numberLength / 2];
                }

                // === Compute intermediary terms === 
                // Compute p = a + b and q = c + d
                Number p = Sum(a, b);
                Number q = Sum(c, d);

                // Compute ac = a * c, bd = b * d, pq = p * q
                Number ac = Product(a, c);
                ac.isPositive = false;
                Number bd = Product(b, d);
                bd.isPositive = false;
                Number pq = Product(p, q);

                // Compute adbc = pq - ac - bd
                Number adbc = Sum(Sum(pq, ac), bd);

                // Pad ac with n zeros
                Number new_ac = new Number();
                // !! Is ac supposed to be numberLength long as well ? !!
                new_ac.digits = new byte[ac.digits.Length + numberLength];
                for (int i = 0; i < ac.digits.Length; i++)
                    new_ac.digits[i] = ac.digits[i];
                for (int i = 0; i < numberLength; i++)
                    new_ac.digits[i + ac.digits.Length] = 0;

                // Pad adbc with n/2 zeros
                Number new_adbc = new Number();
                // !! Is ac supposed to be numberLength long as well ? !!
                new_adbc.digits = new byte[adbc.digits.Length + numberLength/2];
                for (int i = 0; i < adbc.digits.Length; i++)
                    new_adbc.digits[i] = adbc.digits[i];
                for (int i = 0; i < numberLength/2; i++)
                    new_adbc.digits[i + adbc.digits.Length] = 0;

                // Change back the sign of bd
                bd.isPositive = true;
                product = Sum(Sum(new_ac, new_adbc), bd);
            }

            TrimNumber(ref product);
            return product;
        }
        

        // Sum the digits of two equally long numbers. Ignores the sign
        static public byte[] DigitSum(byte[] x, byte[] y)
        {
            byte[] sum = new byte[x.Length];
            byte carryOver = 0;

            if (x.Length == 0)
                throw new Exception("Summing 0-length operators");
            // Add digit by digit and handle the carry over; we avoid 'for' since it's
            //  doing a wraparound (going from 0 down for a byte arrives at 255)
            byte i = (byte)(x.Length - 1);
            do
            {
                byte currentSum = (byte)(x[i] + y[i] + carryOver);
                // Compute the carry over
                if (currentSum > 9)
                {
                    currentSum -= 10;
                    carryOver = 1;
                }
                else
                    carryOver = 0;

                sum[i] = currentSum;

                if (i == 0)
                    break;  // break otherwise the int will wraparound
                else
                    i--;
            } while (i >= 0);

            // allocate +1 slots if final carry over is larger
            byte[] newSum = new byte[sum.Length + 1];
            if (carryOver == 1)
            {
                for (byte k = 0; k < sum.Length; k++)
                {
                    newSum[k + 1] = sum[k];
                }
                newSum[0] = 1;  // the carry over

                sum = newSum;  //check that indeed the new length is larger
            }

            // A quick check to ensure the digits are valid
            for (int k = 0; k < sum.Length; k++)
                if (sum[k] > 9)
                    throw new Exception("Invalid digit !");

            return sum;
        }

        // Substract y from x. The numbers are considered equally long. Ignore the sign of either
        static public byte[] DigitSubstract(byte[] x, byte[] y)
        {
            // A quick check to ensure the digits are valid
            for (int k = 0; k < x.Length; k++)
                if (x[k] > 9)
                    throw new Exception("Invalid digit !");

            // A quick check to ensure the digits are valid
            for (int k = 0; k < y.Length; k++)
                if (y[k] > 9)
                    throw new Exception("Invalid digit !");


            byte[] diff = new byte[x.Length];
            byte nextBorrow = 0;
            byte borrow = 0;
            bool isPositiveResult = true;

            for (int i = x.Length - 1; i >= 0; i--)
            {
                borrow = nextBorrow;
                nextBorrow = 0;
                if (x[i] - borrow >= y[i])      // !check if a negative number results
                {
                    diff[i] = (byte)(x[i] - borrow - y[i]);  // explicit cast due to minus and byte
                }
                else
                {
                    // Just check if there's any non-zero digit further up.
                    //  Whether that digit can be used to substract at that
                    //  point will be determined then, by the first if above
                    bool thereIsAtLeastOneNonZeroDigitAhead = false;
                    for (int k = i - 1; k >= 0; k--)
                        if (x[k] != 0)
                        {
                            thereIsAtLeastOneNonZeroDigitAhead = true;
                            break;
                        }

                    // check if there's still 'credit', and if we can borrow successfully
                    //if (i - 1 >= 0 && x[i - 1] > y[i - 1])
                    if(thereIsAtLeastOneNonZeroDigitAhead)
                    {
                        nextBorrow = 1;
                        diff[i] = (byte)(10 + x[i] - borrow - y[i]);
                    }
                    else
                    {
                        // the result is going to be a negative number; all the work needs
                        //  to be ditched and substraction performed in the other "direction".
                        //  Hopefully only one recursion will be performed
                        isPositiveResult = false;
                        diff = DigitSubstract(y, x);
                        // We have the result, we'll now break
                        break;
                    }
                }

            }

            // Trim the result as needed (note that we could be left with significantly
            //  less digits than what we started with
            byte newStartIndex = 0;
            for (byte i = 0; i < diff.Length; i++)
                if (diff[i] != 0)
                {
                    newStartIndex = i;
                    break;
                }
            byte[] newDiff = new byte[diff.Length - newStartIndex];
            for (byte i = 0; i < diff.Length - newStartIndex; i++)
                newDiff[i] = diff[newStartIndex + i];
            diff = newDiff;

            // A quick check to ensure the digits are valid
            for (int k = 0; k < diff.Length; k++)
                if (diff[k] > 9)
                    throw new Exception("Invalid digit !");

            isSubstractionSignPlus = isPositiveResult;
            return diff;
        }

        // Look at both numbers and if needed pad
        //  the one whose length is smaller, so that
        //  in the end both numbers have the same length
        public static void PadNumber(ref Number x, ref Number y)
        {
            Number numberToPad;  // We'll use a reference-type instance, so we can simplify

            byte differenceInNumberLength = (byte)Math.Abs(x.digits.Length - y.digits.Length);
            if (differenceInNumberLength != 0)
            {
                if (x.digits.Length > y.digits.Length)
                    numberToPad = y;
                else
                    numberToPad = x;

                // Allocate more digit space for the smaller number, so it's as large
                //  as the biggest number
                byte[] newSmallerNumberDigits = new byte[differenceInNumberLength + numberToPad.digits.Length];
                Number newSmallerNumber = new Number();
                newSmallerNumber.digits = newSmallerNumberDigits;

                // Pad the smaller number with 0s in front
                for (int i = 0; i < differenceInNumberLength; i++)
                    newSmallerNumber.digits[i] = 0;
                // Fill in the digits
                for (int i = 0; i < numberToPad.digits.Length; i++)
                {
                    newSmallerNumber.digits[i + differenceInNumberLength] = numberToPad.digits[i];
                }
                // We'll use the reference within numberToPad to overwrite the
                //  digits directly
                numberToPad.digits = newSmallerNumber.digits;
            }

            
        }

        // If found, remove the zero digits that are found
        //  in the beginning of the supplied number
        public static void TrimNumber(ref Number x)
        {
            byte numberLength = (byte)x.digits.Length;

            byte newStartIndex = 0;
            for (byte i = 0; i < x.digits.Length; i++)
                if (x.digits[i] != 0)
                {
                    newStartIndex = i;
                    break;
                }

            if (newStartIndex != 0)
            {
                byte[] newDigits = new byte[numberLength - newStartIndex];
                for (byte i = 0; i < numberLength - newStartIndex; i++)
                    newDigits[i] = x.digits[newStartIndex + i];

                x.digits = newDigits;
            }
        }

        // Increase the length of the numbers supplied to the first power
        //  of 2, if they aren't already there
        public static void UpLengthOfNumberToClosestPowerOfTwo(ref Number x, ref Number y)
        {

            byte lengthOfX = (byte)x.digits.Length;
            byte lengthOfY = (byte)y.digits.Length;

            byte newTargetLength = 0;
            if (lengthOfX > lengthOfY)
            {
                newTargetLength = GetPowerOfTwoAsCeiling(lengthOfX);
            }
            else
            {
                newTargetLength = GetPowerOfTwoAsCeiling(lengthOfY);
            }

            byte[] newDigits;
            if (newTargetLength != lengthOfX)
            {
                newDigits = new byte[newTargetLength];
                // Process X. Pad with enough zeros
                for (int i = 0; i < newTargetLength - lengthOfX; i++)
                    newDigits[i] = 0;
                // copy the former digits
                for (int i = newTargetLength - lengthOfX; i < newTargetLength; i++)
                    newDigits[i] = x.digits[i - newTargetLength + lengthOfX];
                x.digits = newDigits;
            }

            if (newTargetLength != lengthOfY)
            {
                newDigits = new byte[newTargetLength];
                // Process Y. Pad with enough zeros
                for (int i = 0; i < newTargetLength - lengthOfY; i++)
                    newDigits[i] = 0;
                // copy the former digits
                for (int i = newTargetLength - lengthOfY; i < newTargetLength; i++)
                    newDigits[i] = y.digits[i - newTargetLength + lengthOfY];
                y.digits = newDigits;
            }
        }

        // Returns the closest power of 2 that is larger than
        //  the supplied parameter
        public static byte GetPowerOfTwoAsCeiling(byte length)
        {
            byte proposedLength = length;
            while (true)
            {
                bool isPowerOfTwo = true;
                // Determine if the length of x is a power of 2
                for (int i = proposedLength; i > 1; i = i / 2)
                {
                    if (i % 2 != 0)
                    {
                        isPowerOfTwo = false;
                        break;
                    }
                }
                if (isPowerOfTwo)
                {
                    break;
                }
                else
                {
                    // Increase the proposed new length and test again
                    proposedLength++;
                }

            }

            return proposedLength;
        }
    }
}
