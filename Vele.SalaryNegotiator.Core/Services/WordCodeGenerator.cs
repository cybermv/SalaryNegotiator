using System;
using System.Text;
using Vele.SalaryNegotiator.Core.Services.Interfaces;

namespace Vele.SalaryNegotiator.Core.Services
{
    public class WordCodeGenerator : ICodeGenerator
    {
        protected readonly Random _random;

        protected static readonly char[] Vowels = new[] { 'a', 'e', 'i', 'o', 'u', 'y' };
        protected static readonly char[] Consonants = new[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'z' };

        public WordCodeGenerator() : this(new Random()) { }

        public WordCodeGenerator(Random random)
        {
            _random = random;
        }

        public virtual string Generate()
        {
            string code = new StringBuilder()
                .Append(Consonant())
                .Append(Vowel())
                .Append(Consonant())
                .Append(Vowel())
                .Append('-')
                .Append(Consonant())
                .Append(Vowel())
                .Append('-')
                .Append(Consonant())
                .Append(Vowel())
                .Append(Consonant())
                .Append(Vowel())
                .ToString();

            return code;
        }

        private char Vowel() => Vowels[_random.Next(0, Vowels.Length)];

        private char Consonant() => Consonants[_random.Next(0, Consonants.Length)];
    }
}
