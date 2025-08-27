using HotChocolate.Types;
using HotChocolate.Language;
using System.Text.RegularExpressions;

namespace GraphQLSimple.GraphQL.Types
{
    /// <summary>
    /// Custom scalar type for ISBN validation
    /// </summary>
    public class ISBNType : ScalarType<string, StringValueNode>
    {
        public ISBNType() : base("ISBN")
        {
        }

        public override IValueNode ParseResult(object? resultValue)
        {
            if (resultValue is string s && IsValidISBN(s))
            {
                return new StringValueNode(s);
            }

            throw new SerializationException("Invalid ISBN format", this);
        }

        public override bool TrySerialize(object? runtimeValue, out object? resultValue)
        {
            if (runtimeValue is string s && IsValidISBN(s))
            {
                resultValue = s;
                return true;
            }

            resultValue = null;
            return false;
        }

        public override bool TryDeserialize(object? resultValue, out object? runtimeValue)
        {
            if (resultValue is string s && IsValidISBN(s))
            {
                runtimeValue = s;
                return true;
            }

            runtimeValue = null;
            return false;
        }

        protected override string ParseLiteral(StringValueNode valueSyntax)
        {
            if (IsValidISBN(valueSyntax.Value))
            {
                return valueSyntax.Value;
            }

            throw new SerializationException("Invalid ISBN format", this);
        }

        protected override StringValueNode ParseValue(string runtimeValue)
        {
            if (IsValidISBN(runtimeValue))
            {
                return new StringValueNode(runtimeValue);
            }

            throw new SerializationException("Invalid ISBN format", this);
        }

        private static bool IsValidISBN(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn)) return false;
            
            var cleaned = isbn.Replace("-", "").Replace(" ", "");
            return cleaned.Length == 10 || cleaned.Length == 13;
        }
    }

    /// <summary>
    /// Custom scalar type for Email validation
    /// </summary>
    public class EmailType : ScalarType<string, StringValueNode>
    {
        private static readonly Regex EmailRegex = new(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled);

        public EmailType() : base("Email")
        {
        }

        public override IValueNode ParseResult(object? resultValue)
        {
            if (resultValue is string s && IsValidEmail(s))
            {
                return new StringValueNode(s.ToLowerInvariant());
            }

            throw new SerializationException("Invalid email format", this);
        }

        public override bool TrySerialize(object? runtimeValue, out object? resultValue)
        {
            if (runtimeValue is string s && IsValidEmail(s))
            {
                resultValue = s.ToLowerInvariant();
                return true;
            }

            resultValue = null;
            return false;
        }

        public override bool TryDeserialize(object? resultValue, out object? runtimeValue)
        {
            if (resultValue is string s && IsValidEmail(s))
            {
                runtimeValue = s.ToLowerInvariant();
                return true;
            }

            runtimeValue = null;
            return false;
        }

        protected override string ParseLiteral(StringValueNode valueSyntax)
        {
            if (IsValidEmail(valueSyntax.Value))
            {
                return valueSyntax.Value.ToLowerInvariant();
            }

            throw new SerializationException("Invalid email format", this);
        }

        protected override StringValueNode ParseValue(string runtimeValue)
        {
            if (IsValidEmail(runtimeValue))
            {
                return new StringValueNode(runtimeValue.ToLowerInvariant());
            }

            throw new SerializationException("Invalid email format", this);
        }

        private static bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email);
        }
    }

    /// <summary>
    /// Custom scalar type for Phone number validation
    /// </summary>
    public class PhoneType : ScalarType<string, StringValueNode>
    {
        private static readonly Regex PhoneRegex = new(
            @"^\+?[\d\s\-\(\)]+$",
            RegexOptions.Compiled);

        public PhoneType() : base("Phone")
        {
        }

        public override IValueNode ParseResult(object? resultValue)
        {
            if (resultValue is string s && IsValidPhone(s))
            {
                return new StringValueNode(s);
            }

            throw new SerializationException("Invalid phone number format", this);
        }

        public override bool TrySerialize(object? runtimeValue, out object? resultValue)
        {
            if (runtimeValue is string s && IsValidPhone(s))
            {
                resultValue = s;
                return true;
            }

            resultValue = null;
            return false;
        }

        public override bool TryDeserialize(object? resultValue, out object? runtimeValue)
        {
            if (resultValue is string s && IsValidPhone(s))
            {
                runtimeValue = s;
                return true;
            }

            runtimeValue = null;
            return false;
        }

        protected override string ParseLiteral(StringValueNode valueSyntax)
        {
            if (IsValidPhone(valueSyntax.Value))
            {
                return valueSyntax.Value;
            }

            throw new SerializationException("Invalid phone number format", this);
        }

        protected override StringValueNode ParseValue(string runtimeValue)
        {
            if (IsValidPhone(runtimeValue))
            {
                return new StringValueNode(runtimeValue);
            }

            throw new SerializationException("Invalid phone number format", this);
        }

        private static bool IsValidPhone(string phone)
        {
            return !string.IsNullOrWhiteSpace(phone) && PhoneRegex.IsMatch(phone);
        }
    }

    /// <summary>
    /// Custom scalar type for URL validation
    /// </summary>
    public class URLType : ScalarType<string, StringValueNode>
    {
        public URLType() : base("URL")
        {
        }

        public override IValueNode ParseResult(object? resultValue)
        {
            if (resultValue is string s && Uri.TryCreate(s, UriKind.Absolute, out _))
            {
                return new StringValueNode(s);
            }

            throw new SerializationException("Invalid URL format", this);
        }

        public override bool TrySerialize(object? runtimeValue, out object? resultValue)
        {
            if (runtimeValue is string s && Uri.TryCreate(s, UriKind.Absolute, out _))
            {
                resultValue = s;
                return true;
            }

            resultValue = null;
            return false;
        }

        public override bool TryDeserialize(object? resultValue, out object? runtimeValue)
        {
            if (resultValue is string s && Uri.TryCreate(s, UriKind.Absolute, out _))
            {
                runtimeValue = s;
                return true;
            }

            runtimeValue = null;
            return false;
        }

        protected override string ParseLiteral(StringValueNode valueSyntax)
        {
            if (Uri.TryCreate(valueSyntax.Value, UriKind.Absolute, out _))
            {
                return valueSyntax.Value;
            }

            throw new SerializationException("Invalid URL format", this);
        }

        protected override StringValueNode ParseValue(string runtimeValue)
        {
            if (Uri.TryCreate(runtimeValue, UriKind.Absolute, out _))
            {
                return new StringValueNode(runtimeValue);
            }

            throw new SerializationException("Invalid URL format", this);
        }
    }

    /// <summary>
    /// Custom scalar type for positive integers
    /// </summary>
    public class PositiveIntType : ScalarType<int, IntValueNode>
    {
        public PositiveIntType() : base("PositiveInt")
        {
        }

        public override IValueNode ParseResult(object? resultValue)
        {
            if (resultValue is int i && i > 0)
            {
                return new IntValueNode(i);
            }

            throw new SerializationException("Value must be a positive integer", this);
        }

        public override bool TrySerialize(object? runtimeValue, out object? resultValue)
        {
            if (runtimeValue is int i && i > 0)
            {
                resultValue = i;
                return true;
            }

            resultValue = null;
            return false;
        }

        public override bool TryDeserialize(object? resultValue, out object? runtimeValue)
        {
            if (resultValue is int i && i > 0)
            {
                runtimeValue = i;
                return true;
            }

            runtimeValue = null;
            return false;
        }

        protected override int ParseLiteral(IntValueNode valueSyntax)
        {
            if (valueSyntax.ToInt32() > 0)
            {
                return valueSyntax.ToInt32();
            }

            throw new SerializationException("Value must be a positive integer", this);
        }

        protected override IntValueNode ParseValue(int runtimeValue)
        {
            if (runtimeValue > 0)
            {
                return new IntValueNode(runtimeValue);
            }

            throw new SerializationException("Value must be a positive integer", this);
        }
    }
}