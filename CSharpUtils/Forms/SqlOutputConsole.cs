using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SoftLife.CSharp
{
    /// <summary>
    /// Consola que mostra o output de comandos SQL associados a uma SqlConnection.
    /// </summary>
    public partial class SqlOutputConsole : Form
    {
        /// <summary>
        /// Ligação à BD. Escuta e mostra prints das queries executadas nesta connection.
        /// </summary>
        public SqlConnection DBConnection;

        private SqlInfoMessageEventHandler DBOutputHandler;        
        private OutputSeparator Separator;

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="dbConnection">Ligação à base de dados a escutar. Imprime todas as mensagens retornadas.</param>
        /// <param name="commandSeparator">Linha separadora entre o output de dois comandos.</param>
        public SqlOutputConsole(SqlConnection dbConnection, OutputSeparator commandSeparator = CSharp.OutputSeparator.LightDashLine)
        {
            InitializeComponent();
            this.DBConnection = dbConnection;
            this.Separator = commandSeparator;
            this.DBOutputHandler = new SqlInfoMessageEventHandler(DBConnection_InfoMessage);

            DBConnection.InfoMessage += DBOutputHandler;
        }

        /// <summary>
        /// Altera as dimensões da consola.
        /// </summary>
        /// <param name="width">Nova largura</param>
        /// <param name="height">Nova altura</param>
        /// <returns>O próprio objecto depois de alterado.</returns>
        public SqlOutputConsole SetSize(int width, int height)
        {
            this.Size = new Size(width, height);
            return this;
        }

        /// <summary>
        /// Imprime na consola o separador definido.
        /// </summary>
        public void AppendSeparatorLine()
        {
            AppendLine(StringEnum.GetStringValue(Separator));
        }

        /// <summary>
        /// Imprime na consola uma mensagem (com mudança de linha no fim).
        /// </summary>
        /// <param name="message">Mensagem a mostrar</param>
        public void AppendLine(string message)
        {
            Append(message + Environment.NewLine);
        }

        /// <summary>
        /// Imprime na consola uma mensagem.
        /// </summary>
        /// <param name="message">Mensagem a mostrar</param>
        public void Append(string message)
        {
            txtOutputConsole.AppendText(message);
        }

        #region Eventos

        private void SqlOutputConsole_FormClosing(object sender, FormClosingEventArgs e)
        {
            DBConnection.InfoMessage -= DBOutputHandler;
        }
        
        private void DBConnection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            AppendLine(e.Message);
        }

        /// <summary>
        /// Quando texto é acrescentado, é feito automaticamente scroll para o fim.
        /// </summary>
        private void txtOutputConsole_TextChanged(object sender, System.EventArgs e)
        {
            txtOutputConsole.SelectionStart = txtOutputConsole.TextLength;
            txtOutputConsole.ScrollToCaret();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyPressed)
        {
            if (keyPressed == Keys.Escape)
            {
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyPressed);
        }

        #endregion
    }
}
