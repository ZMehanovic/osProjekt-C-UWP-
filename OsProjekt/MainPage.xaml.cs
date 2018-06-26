using System;
using System.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace OsProjekt
{
    public sealed partial class MainPage : Page
    {
        private const String _secretKey = "tajni_kljuc.txt";
        private const String _privateKey = "privatni_kljuc.txt";
        private const String _publicKey = "javni_kljuc.txt";
        private const String _encryptedTextAes = "kriptirano_aes.txt";
        private const String _encryptedTextRsa = "kriptirano_rsa.txt";
        private const String _messageDigest = "sazetak.txt";
        private const String _initialText = "ulazna_datoteka.txt";
        private const String _signature = "potpis.txt";


        private String fileContent = "";
        private IStorageFile inputFile;
        private Boolean isAES = true;
        private Aes aes;
        private RSA rsa;
        private byte[] encryptedByteArrayAes;
        private byte[] encryptedByteArrayRsa;

        public MainPage() => InitializeComponent();

        private async void SetDefaultValues()
        {
            IStorageFile file = await FilesHelper.GetFile(_initialText);
            EncryptionTextBox.Text = await FileIO.ReadTextAsync(file);
        }

        private async void FileSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilesHelper.DataFolder == null)
            {
                ShowMessageDialog();
                return;
            }
            var filePicker = new Windows.Storage.Pickers.FileOpenPicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };

            filePicker.FileTypeFilter.Add(".txt");

            inputFile = await filePicker.PickSingleFileAsync();
            if (inputFile != null)
            {
                fileContent = await FileIO.ReadTextAsync(inputFile,
                    encoding: Windows.Storage.Streams.UnicodeEncoding.Utf8);

                EncryptionTextBox.Text = fileContent;
                SelectedFile.Text = inputFile.Name;
            }
        }
        private async void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilesHelper.DataFolder == null)
            {
                ShowMessageDialog();
                return;
            }
            IStorageFile encryptedFile;
            if (isAES)
            {
                aes = Aes.Create();
                IStorageFile secretKeyFile = await FilesHelper.GetFile(_secretKey);
                await FileIO.WriteBytesAsync(file: secretKeyFile, buffer: aes.Key);

                encryptedFile = await FilesHelper.GetFile(_encryptedTextAes);
                encryptedByteArrayAes = CryptographyHelper.EncryptAes(fileContent, aes);
                await FileIO.WriteBytesAsync(file: encryptedFile, buffer: encryptedByteArrayAes);
                ResultTextBlock.Text = await FileIO.ReadTextAsync(encryptedFile,
                    Windows.Storage.Streams.UnicodeEncoding.Utf16BE);
            }
            else
            {
                rsa = RSA.Create();
                await CreateRsaKeys();

                encryptedFile = await FilesHelper.GetFile(_encryptedTextRsa);
                encryptedByteArrayRsa = CryptographyHelper.EncryptRSA(
                    await FilesHelper.GetByteArrayAsync(_initialText), rsa.ExportParameters(false));

                await FileIO.WriteBytesAsync(file: encryptedFile, buffer: encryptedByteArrayRsa);
                ResultTextBlock.Text = await FileIO.ReadTextAsync(encryptedFile,
                    UnicodeEncoding.Utf16BE);

            }
        }

        private async void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            if (isAES && aes != null)
            {
                ResultTextBlock.Text = CryptographyHelper.DecryptAes(
                    await FilesHelper.GetByteArrayAsync(_encryptedTextAes), aes);
            }
            else if (!isAES && rsa != null)
            {
                ResultTextBlock.Text = CryptographyHelper.DecryptRSA(
                     await FilesHelper.GetByteArrayAsync(_encryptedTextRsa), rsa.ExportParameters(true), false);

            }
        }
       
        private async void MessageDigestButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilesHelper.DataFolder == null)
            {
                ShowMessageDialog();
                return;
            }

            String hash = CryptographyHelper.Sha256Hash(fileContent);
            IStorageFile hashFile = await FilesHelper.GetFile(_messageDigest);
            await FileIO.WriteTextAsync(hashFile, hash);

            ResultTextBlock.Text = hash;
        }

        private async void SignButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilesHelper.DataFolder == null)
            {
                ShowMessageDialog();
                return;
            }

            String signedText = CryptographyHelper.CreateSignature(fileContent);

            IStorageFile signedFile = await FilesHelper.GetFile(_signature);
            await FileIO.WriteTextAsync(signedFile, signedText);
            ResultTextBlock.Text = signedText;

        }

        private async void CheckSignatureButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilesHelper.DataFolder == null)
            {
                ShowMessageDialog();
                return;
            }

            String inputFileText = await FileIO.ReadTextAsync(inputFile);
            IStorageFile signatureFile = await FilesHelper.GetFile(_signature);
            String signature = await FileIO.ReadTextAsync(signatureFile);

            if (CryptographyHelper.VerifySignature(inputFileText, signature))
            {
                ResultTextBlock.Text = "Potpis je ispravan";
            }
            else
            {
                ResultTextBlock.Text = "Potpis nije ispravan";
            }
        }



        private void EncryptionType_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch toggleSwitch && toggleSwitch.IsOn)
            {
                isAES = false;
            }
            else
            {
                isAES = true;
            }
        }

        private async void SelectWorkingDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };
            folderPicker.FileTypeFilter.Add(".csv");

            FilesHelper.DataFolder = await folderPicker.PickSingleFolderAsync();

            if (FilesHelper.DataFolder != null)
            {
                SetDefaultValues();
            }
        }
        private async System.Threading.Tasks.Task CreateRsaKeys()
        {
            Chilkat.Rsa rsa = new Chilkat.Rsa();
            rsa.UnlockComponent("Anything for 30-day trial");
            rsa.GenerateKey(1024);
            rsa.EncodingMode = "hex";
            rsa.LittleEndian = false;

            string publicKey = rsa.ExportPublicKey();
            IStorageFile publicKeyFile = await FilesHelper.GetFile(_publicKey);
            await FileIO.WriteTextAsync(publicKeyFile, publicKey);

            string privateKey = rsa.ExportPrivateKey();
            IStorageFile privateKeyFile = await FilesHelper.GetFile(_privateKey);
            await FileIO.WriteTextAsync(privateKeyFile, privateKey);

        }

        private async void EncryptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FilesHelper.DataFolder == null)
            {
                ShowMessageDialog();
                return;
            }
            if (sender is TextBox contentTextBox)
            {
                fileContent = contentTextBox.Text;
                inputFile = await FilesHelper.GetFile(_initialText);
                await FileIO.WriteTextAsync(inputFile, fileContent);
            }
        }

        private async void ShowMessageDialog()
        {
            await new Windows.UI.Popups.MessageDialog("Direktorij nije odabran", "Upozorenje").ShowAsync();
        }
    }
}
