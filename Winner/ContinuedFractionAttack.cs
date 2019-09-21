using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Wiener
{
    public class ContinuedFractionAttack
    {
        List<BigInteger> m_lstD = new List<BigInteger>();
        private List<BigInteger> m_lstContinuedFraction;
        private RsaKey m_rsaKey;
        private WienerMainForm m_form;

        public ContinuedFractionAttack(BigInteger e, BigInteger n)
        {
            m_rsaKey = new RsaKey(e, n);
            m_lstContinuedFraction = new List<BigInteger>();
            m_form = Program.m_form;
        }

        private List<BigInteger> FractiontoContinuedFraction(BigFraction _fraction)
        {
            List<BigInteger> _lstCF = new List<BigInteger>();
            List<BigFraction> _lstFraction = new List<BigFraction>();
            BigInteger _remainder; 
            BigInteger _qi;
            
            _qi = BigInteger.DivRem(_fraction.Numerator, _fraction.Denominator, out _remainder);
            _lstCF.Add(_qi);
            BigFraction _bfrac = new BigFraction(_remainder, _fraction.Denominator);
            _lstFraction.Add(_bfrac);
            int i = 0;
            while (true)
            {
                i++;
                _qi = BigInteger.DivRem(_lstFraction[i - 1].Denominator, _lstFraction[i - 1].Numerator, out _remainder);
                _lstCF.Add(_qi);
                _bfrac = new BigFraction(_remainder, _lstFraction[i - 1].Numerator);
                _lstFraction.Add(_bfrac);
                if (_remainder <= BigInteger.Zero)
                {
                    break;
                }
            }
            return _lstCF;
        }
        
        private BigFraction ContinuedFractiontoFraction(List<BigInteger> _contfraction)
        {
            BigFraction _bfraction = new BigFraction();
            List<BigInteger> _lstNumerator = new List<BigInteger>();
            List<BigInteger> _lstDenominator = new List<BigInteger>();
            int _iCount = _contfraction.Count;
            _lstNumerator.Add(_contfraction[0]);
            _lstDenominator.Add(1);
            if (_iCount > 1)
            {
                _lstNumerator.Add(_contfraction[0] * _contfraction[1] + 1);
                _lstDenominator.Add(_contfraction[1]);
                for (int i = 2; i < _iCount; i++)
                {
                    _lstNumerator.Add(_contfraction[i] * _lstNumerator[i - 1] + _lstNumerator[i - 2]);
                    _lstDenominator.Add(_contfraction[i] * _lstDenominator[i - 1] + _lstDenominator[i - 2]);
                }
            }
            _bfraction.Numerator = _lstNumerator.Last();
            _bfraction.Denominator = _lstDenominator.Last();

            return _bfraction;
        }

        public BigInteger WienerAlgorithm()
        {
            m_form.UpdateListbox("Calculating continued fraction from e/N");
            BigFraction _bfraction = new BigFraction(m_rsaKey.Public_exponent, m_rsaKey.Modulus);
            m_lstContinuedFraction = FractiontoContinuedFraction(_bfraction);
            List<BigInteger> tmp_lstContFraction = new List<BigInteger>();
            tmp_lstContFraction = m_lstContinuedFraction.Clone().ToList();
            int iCount = m_lstContinuedFraction.Count;

            m_form.UpdateListbox("Continued fraction from e/N is:");
            string _strContFraction = null;
            for (int i = 0; i < iCount; i++)
            {
                if (i == 0)
                    _strContFraction = "< ";
                _strContFraction += m_lstContinuedFraction[i];
                if (i == iCount - 1)
                    _strContFraction +=  " >";
                else
                 _strContFraction += ", ";
            }
            m_form.UpdateListbox(_strContFraction);

            for (int i = 0; i < iCount; i++)
            {
                BigFraction _bfrK_DG = new BigFraction();
                BigFraction _bfrEulerToitient;

                tmp_lstContFraction = m_lstContinuedFraction.Take(i + 1).ToList();

                if (0 == i % 2)
                {
                    tmp_lstContFraction[i] = tmp_lstContFraction[i] + 1;
                }

                _bfrK_DG = ContinuedFractiontoFraction(tmp_lstContFraction);
                 _bfrEulerToitient = new BigFraction(_bfrK_DG.Denominator * m_rsaKey.Public_exponent - 1, _bfrK_DG.Numerator);

                BigInteger _iEulerToitient = _bfrEulerToitient.Floor();

                var result = SolveQuadraticEquation(1, -1 * (m_rsaKey.Modulus - _iEulerToitient + 1), m_rsaKey.Modulus);
                BigInteger _iG = BigInteger.Remainder(_bfrEulerToitient.Numerator, _bfrEulerToitient.Denominator);

            
                if (result.Item1 * result.Item2 == m_rsaKey.Modulus)
                {
                    m_rsaKey.Private_exponent = _bfrK_DG.Denominator;
                    m_lstD.Add(m_rsaKey.Private_exponent);
                    if (m_rsaKey.TestKey())
                        return m_rsaKey.Private_exponent;
                }


            }

            return BigInteger.Zero;
        }

        public Tuple<BigInteger, BigInteger> SolveQuadraticEquation(BigInteger a, BigInteger b, BigInteger c)
        {
            BigInteger discRoot = (b * b - 4 * a * c).Sqrt();
            BigInteger x1 = (-b + discRoot) / (2 * a);
            BigInteger x2 = (-b - discRoot) / (2 * a);
            return Tuple.Create(x1, x2);
        }

        private void sendMessage(string _msg)
        {
            this.m_form.UpdateListbox(_msg);
        }

    }

    public static class Extentions
    {
        public static T Clone<T>(this T original) where T : class 
        {
            using(MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, original);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }
    }

    public static class BigIntExtension
    {
        public static BigInteger Sqrt(this BigInteger n)
        {
            if (n == 0) return 0;
            if (n > 0)
            {
                int bitLength = Convert.ToInt32(Math.Ceiling(BigInteger.Log(n, 2)));
                BigInteger root = BigInteger.One << (bitLength / 2);

                while (!isSqrt(n, root))
                {
                    root += n / root;
                    root /= 2;
                }

                return root;
            }

            throw new ArithmeticException("NaN");
        }

        private static Boolean isSqrt(BigInteger n, BigInteger root)
        {
            BigInteger lowerBound = root * root;
            BigInteger upperBound = (root + 1) * (root + 1);

            return (n >= lowerBound && n < upperBound);
        }
    }

}
