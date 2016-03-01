using System.Windows.Forms;

namespace SoftLife.CSharp
{
    /// <summary>
    /// Janela com barra de progresso e descrição por baixo.
    /// </summary>
    public partial class ProgressBarWindow : Form
    {
        /// <summary>
        /// Constrói uma janela de progresso.
        /// </summary>
        /// <param name="description">Descrição a mostrar abaixo da barra de progresso.</param>
        public ProgressBarWindow(string description = "A processar...")
        {
            InitializeComponent();
            progressBar.Value = 0;
            progressDescription.Text = description;
        }
        
        /// <summary>
        /// Altera o progresso.
        /// </summary>
        /// <param name="totalWorkComplete">Inteiro entre 0 e 100 que representa o progresso do trabalho.</param>
        /// <param name="nextTaskDescription">Descrição da próxima tarefa, para mostrar abaixo do progresso.</param>
        public void SetProgress(int totalWorkComplete, string nextTaskDescription = "")
        {
            progressBar.Value = totalWorkComplete;
            if (nextTaskDescription != "")
                progressDescription.Text = nextTaskDescription;
        }

        /// <summary>
        /// Mostra a janela de progresso, i.e. invoca Show().
        /// </summary>
        public void Start()
        {
            this.Show();
        }

        /// <summary>
        /// Fecha a janela de progresso após completar barra, i.e. invoca Close().
        /// </summary>
        /// <param name="description">Mensagem a mostrar ao terminar o progresso</param>
        public void End(string description = "Terminado")
        {
            SetProgress(100, description);            
            this.Close();
        }
    }
}
