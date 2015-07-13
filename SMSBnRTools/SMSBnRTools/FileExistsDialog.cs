using System.Windows.Forms;

namespace SMSBnRTools
{
    public partial class FileExistsDialog : Form
    {
        public FileExistsDialog()
        {
            InitializeComponent();
        }

        public FileExistsDialog(string message)
        {
            InitializeComponent();
            labelPrompt.Text = message;
        }
    }
}
