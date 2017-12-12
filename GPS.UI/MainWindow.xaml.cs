using GPS.UI.Properties;
using ProtoBuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GPS.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string, int> dictionary;
        private Task loadDictionaryTask;
        private ConcurrentDictionary<string, int> matchedWords;

        public MainWindow()
        {
            InitializeComponent();

            this.matchedWords = new ConcurrentDictionary<string, int>();
            this.loadDictionaryTask = Task.Run(() =>
            {
                using (FileStream stream = File.OpenRead(Settings.Default.DictionaryPath))
                {
                    this.dictionary = Serializer.Deserialize<Dictionary<string, int>>(stream);
                }

                this.statusLabel.Dispatcher.Invoke(() =>
                {
                    this.statusLabel.Content = "Dictionary loaded!";
                });
            });

            this.statusLabel.Content = "Loading Dictionary...";
        }

        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (loadDictionaryTask == null || loadDictionaryTask.Status != TaskStatus.RanToCompletion)
            {
                return;
            }

            const int maxWordsMatched = 500;
            matchedWords.Clear();

            // For a first run, perform a simple comparison assuming * at the end.
            string searchText = this.searchBox.Text;

            Parallel.ForEach(this.dictionary, (kvp, loopState) =>
            {
                if (kvp.Key.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    matchedWords.AddOrUpdate(kvp.Key, kvp.Value, (word, count) => kvp.Value);
                    if (matchedWords.Count >= maxWordsMatched)
                    {
                        loopState.Stop();
                    }
                }
            });

            this.statusLabel.Content = $"Found {matchedWords.Count}{(matchedWords.Count >= maxWordsMatched ? "+" : "")} word{(matchedWords.Count != 1 ? "s" : "")} in the dictionary.";
            this.resultsBox.Items.Clear();
            foreach (KeyValuePair<string, int> word in matchedWords)
            {
                this.resultsBox.Items.Add($"{word.Key}: {string.Format("{0:N0}", word.Value)}");
            }
        }
    }
}
