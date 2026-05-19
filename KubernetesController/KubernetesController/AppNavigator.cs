using System.Windows.Forms;

namespace KubernetesController
{
    public static class AppNavigator
    {
        public static ApplicationContext Context { get; } = new ApplicationContext();

        private static Form currentForm;
        private static bool switchingForm = false;

        public static void Start(Form form)
        {
            currentForm = form;
            form.FormClosed += OnFormClosed;
            form.Show();
        }

        public static void NavigateTo(Form nextForm)
        {
            switchingForm = true;

            Form oldForm = currentForm;
            currentForm = nextForm;

            nextForm.FormClosed += OnFormClosed;
            nextForm.Show();

            oldForm?.Close();

            switchingForm = false;
        }

        private static void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            if (!switchingForm && sender == currentForm)
            {
                Context.ExitThread();
            }
        }
    }
}