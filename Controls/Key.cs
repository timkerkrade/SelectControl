using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Controls
{
    [Serializable()]
    public class Key<T>
    {
        private Key()
        {
        }

        public Key(bool other, string text)
        {
            Option = KeyOption.other;
            Text = text;
        }

        public Key(T value)
        {
            Value = value;
        }

        public Key(Key<T> otherKey)
        {
            if (otherKey.Value is KeyId)
            {
                Type t1 = otherKey.Value.GetType();
                var newValue = Activator.CreateInstance(t1);

                foreach (var prp in otherKey.Value.GetType().GetProperties())
                {
                    prp.SetValue(newValue, prp.GetValue(otherKey.Value, null), null);
                }
                Value = (T)newValue;
            }
            else
            {
                Value = otherKey.Value;
            }
            Option = otherKey.Option;
        }

        public Key<T> GetLevelledKey(int level)
        {
            var newKey = new Key<T>(this);
            newKey.KeyLevel = level;
            return newKey;
        }

        public static Key<T> CreateKey(string keyString)
        {
            var result = new Key<T>();
            var keyArray = keyString.Split(keySeparator);

            if (keyArray.Length == 4)
            {
                result.KeyLevel = int.Parse(keyArray[0], CultureInfo.InvariantCulture);
                result.Unchecked = Boolean.Parse(keyArray[1]);

                JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
                result.Value = (T)jsSerializer.Deserialize<T>(keyArray[2]);

                switch (keyArray[3])
                {
                    case "normal":
                        result.Option = KeyOption.normal;
                        break;
                    case "newitem":
                        result.Option = KeyOption.newitem;
                        break;
                    case "more":
                        result.Option = KeyOption.more;
                        break;
                    case "other":
                        result.Option = KeyOption.other;
                        break;
                    case "inRetentie":
                        result.Option = KeyOption.inRetentie;
                        break;
                }
                return result;
            }
            else
            {
                throw new InvalidOperationException("no valid keystring supplied");
            }
        }

        protected const char keySeparator = ';';

        public int KeyLevel
        {
            get
            {
                if (Value is KeyId)
                {
                    return (Value as KeyId).Level;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (Value is KeyId)
                {
                    (Value as KeyId).Level = value;
                }
            }
        }

        public void SetNew()
        {
            Option = KeyOption.newitem;
            KeyLevel++;
        }

        public void SetMore()
        {
            Option = KeyOption.more;
        }

        public KeyOption Option { get; set; }
        public T Value { get; set; }
        public string Text { get; set; }

        /// <summary>
        /// boolean om aan te geven dat het niet zeker is dat er data bestaat voor deze key
        /// </summary>
        public bool Unchecked { get; set; }

        public override string ToString()
        {
            var returnValue = new StringBuilder();
            returnValue.Append(KeyLevel.ToString(CultureInfo.InvariantCulture) + keySeparator);
            returnValue.Append(Unchecked.ToString(CultureInfo.InvariantCulture) + keySeparator);

            var jsSerializer = new JavaScriptSerializer();
            returnValue.Append(jsSerializer.Serialize(Value) + keySeparator);
            returnValue.Append(Option.ToString());
            return returnValue.ToString();
        }

        /// <summary>
        /// compare two keys. 
        /// </summary>
        /// <param name="one"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool operator ==(Key<T> one, Key<T> other)
        {
            // if both are null means equal
            if ((object)one == null && (object)other == null)
            {
                return true;
            }

            // if only one is null means not equal
            if ((object)one == null || (object)other == null)
            {
                return false;
            }

            return one.ToString() == other.ToString();
        }

        public static bool operator !=(Key<T> one, Key<T> other)
        {
            return !(one == other);
        }

        public bool Contains(Key<T> otherKey)
        {
            if (KeyLevel < otherKey.KeyLevel)
            {
                var levelledOtherKey = otherKey.GetLevelledKey(KeyLevel);
                return this == levelledOtherKey;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            return this.ToString() == obj.ToString();
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }

    public enum KeyOption
    {
        normal,
        newitem,
        more,
        other,
        inRetentie
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class LevelAttribute : Attribute
    {
        private int value;

        public LevelAttribute(int value)
        {
            this.value = value;
        }

        public int Value
        {
            get { return this.value; }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IgnoreAttribute : Attribute
    {
        private bool value;

        public IgnoreAttribute(bool value)
        {
            this.value = value;
        }

        public bool Value
        {
            get { return this.value; }
        }
    }
}
