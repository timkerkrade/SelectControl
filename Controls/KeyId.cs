using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controls
{
    [Serializable()]
    public class KeyId
    {
        public int Level
        {
            get
            {
                int maxLevelFound = -1;
                foreach (var prp in this.GetType().GetProperties())
                {
                    var attributes = prp.GetCustomAttributes(true);
                    foreach (var attribute in attributes)
                    {
                        LevelAttribute la = attribute as LevelAttribute;

                        if (la != null)
                        {
                            if (la.Value > maxLevelFound && prp.GetValue(this, null) != null)
                            {
                                maxLevelFound = la.Value;
                            }
                        }
                    }
                }
                return maxLevelFound;
            }
            set
            {
                foreach (var prp in this.GetType().GetProperties())
                {
                    var attributes = prp.GetCustomAttributes(true);
                    foreach (var attribute in attributes)
                    {
                        LevelAttribute la = attribute as LevelAttribute;

                        if (la != null)
                        {
                            if (la.Value > value && prp.GetValue(this, null) != null)
                            {
                                prp.SetValue(this, null, null);
                            }
                        }
                    }
                }
            }
        }
    }
}
