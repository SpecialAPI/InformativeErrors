using System;
using System.Collections.Generic;
using System.Text;

namespace InformativeErrors
{
    public static class BasegameToStringExtensions
    {
        public static string EffectInfoToString(EffectInfo effect)
        {
            var st = "Null Effect";

            if(effect.effect != null)
                st = string.IsNullOrEmpty(effect.effect.name) ? effect.effect.GetType().ToString() : effect.effect.name;

            st += $", {effect.entryVariable}";

            if (effect.targets != null)
                st += $", {(string.IsNullOrEmpty(effect.targets.name) ? effect.targets.GetType().ToString() : effect.targets.name)}";

            if(effect.condition != null)
                st += $", {(string.IsNullOrEmpty(effect.condition.name) ? effect.condition.GetType().ToString() : effect.condition.name)}";

            return st;
        }
    }
}
