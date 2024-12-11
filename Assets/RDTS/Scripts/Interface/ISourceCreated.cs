using System.Collections.Generic;

namespace RDTS
{
    //!  Interface for Event Method by Source. On SourceCreated is called on all components implementing ISourceCreated
    public interface ISourceCreated
    {
        void OnSourceCreated();
    }
}
