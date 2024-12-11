//! Interface for all Classes which are Fixing components (currently Grip and Fixer)
namespace RDTS
{
    public interface IFix
    {
        void Fix(MU mu);
        void Unfix(MU mu);
    }
}