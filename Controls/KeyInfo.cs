using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controls
{
    public class KeyInfo
    {
        protected const char keySeparator = ';';

        public static KeyInfo CreateKeyInfo(string keyString)
        {
            var keyArray = keyString.Split(keySeparator);

            if (keyArray.Length == 4)
            {
                var ki = new KeyInfo();
                switch (keyArray[3])
                {
                    case "normal":
                        break;
                    case "newitem":
                        ki.New = true;
                        break;
                    case "more":
                        ki.More = true;
                        break;
                    case "other":
                        ki.Other = true;
                        break;
                }
                return ki;
            }
            else
            {
                throw new InvalidOperationException("no valid keystring supplied");
            }
        }

        private bool isMore = false;
        private bool isOther = false;
        private bool isNew = false;
        private int level = 0;
        private string text;

        public bool More
        {
            get { return isMore; }
            set { isMore = value; }
        }

        public bool Other
        {
            get { return isOther; }
            set { isOther = value; }
        }

        public bool New
        {
            get { return isNew; }
            set { isNew = value; }
        }

        public int Level
        {
            get { return level; }
            set { level = value; }
        }

        public string Text
        {
            get { return text; }
            set { text = value; }
        }
    }
}
