using UnityEngine;

namespace ODev.VariableSOs
{
    public abstract class ValidatorSO : ScriptableObject
    {
        public abstract bool IsValid();
    }
}
