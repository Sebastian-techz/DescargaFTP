using System;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace DescargaFTP
{
    public partial class Form1 : Form
    {
        private string selectedFilePath; // Para almacenar la ruta del archivo seleccionado
        private string destinationPath;   // Para almacenar la ruta de destino
        public Form1()
        {
            InitializeComponent();
        }

        private void selectFileButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Selecciona un archivo para descargar";
                openFileDialog.Filter = "Todos los archivos (*.*)|*.*"; // Puedes ajustar el filtro

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFilePath = openFileDialog.FileName; // Guardar la ruta del archivo seleccionado
                    filePathLabel.Text = $"Archivo seleccionado: {Path.GetFileName(selectedFilePath)}"; // Mostrar el nombre del archivo
                }
            }
        }

        private void selectDestinationButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    destinationPath = folderDialog.SelectedPath; // Guardar la ruta de destino
                    destinationLabel.Text = $"Ruta de destino: {destinationPath}"; // Mostrar la ruta
                }
            }
        }

        private void downloadButton_Click(object sender, EventArgs e)
        {
            string ftpServer = serverTextBox.Text.Trim(); // Obtener la dirección del servidor y eliminar espacios
            string ftpUsername = usernameTextBox.Text.Trim(); // Obtener el nombre de usuario
            string ftpPassword = passwordTextBox.Text.Trim(); // Obtener la contraseña

            // Validar si se han seleccionado el archivo y la ruta de destino
            if (string.IsNullOrEmpty(selectedFilePath) || string.IsNullOrEmpty(destinationPath))
            {
                MessageBox.Show("Por favor, selecciona un archivo y una ruta de destino.");
                return;
            }

            // Obtener el nombre del archivo y codificarlo
            string fileName = Path.GetFileName(selectedFilePath);
            string encodedFileName = Uri.EscapeDataString(fileName); // Codificar el nombre del archivo
            string uriString = $"ftp://{ftpServer}/{encodedFileName}";

            // Mostrar la URL para verificar
            MessageBox.Show("URL a descargar: " + uriString);

            // Validar que la URL sea válida
            if (!Uri.IsWellFormedUriString(uriString, UriKind.Absolute))
            {
                MessageBox.Show("La URL del servidor FTP no es válida: " + uriString);
                return;
            }

            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
                    webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                    webClient.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(DownloadCompleted);

                    // Iniciar la descarga
                    string localFilePath = Path.Combine(destinationPath, fileName);
                    webClient.DownloadFileAsync(new Uri(uriString), localFilePath);
                    statusLabel.Text = "Descargando...";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en la descarga: " + ex.Message);
            }
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage; // Actualizar la barra de progreso
            statusLabel.Text = $"Progreso: {e.ProgressPercentage}%"; // Mostrar el progreso
        }

        private void DownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("Error en la descarga: " + e.Error.Message);
            }
            else
            {
                MessageBox.Show("Descarga completada.");
                statusLabel.Text = "Descarga completada.";
            }

            // Resetear la barra de progreso
            //progressBar.Value = 0;
        }

    }
}
