using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Wiener
{
    public class BigFraction
    {
        private BigInteger numerator;        
        private BigInteger denominator;

        public BigInteger Numerator
        {
            get { return numerator; }
            set { numerator = value; }
        }
        public BigInteger Denominator
        {
            get { return denominator; }
            set { denominator = value; }
        }

        public BigFraction()
        {
            numerator = 0;
            denominator = 1;
        }
        public BigFraction(BigInteger _numberator, BigInteger _denominator)
        {
            numerator = _numberator;
            denominator = _denominator;
        }

        public BigInteger Round()
        {
            if (BigInteger.Remainder(Numerator, Denominator) * 2 < Denominator)
                return BigInteger.Divide(Numerator, Denominator);
            else
                return BigInteger.Divide(Numerator, Denominator) + 1;
        }

        public BigInteger Floor()
        {
            return BigInteger.Divide(Numerator, Denominator);
        }
    }
}
