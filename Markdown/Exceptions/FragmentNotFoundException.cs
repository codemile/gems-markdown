using Markdown.Documents;

namespace Markdown.Exceptions
{
    public class FragmentNotFoundException : FragmentException
    {
        public FragmentNotFoundException(string pName, DocumentCursor pCursor)
            : base(string.Format("Fragment {0} was not found.", pName), pCursor)
        {
        }
    }
}