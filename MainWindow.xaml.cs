using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace VBfoci
{
    public partial class MainWindow : Window
    {

        /* 
           Horvai Dániel feladatai: a,b,d,g
           Lantos Péter feladatai: c,e,f,h
        */
        List<Resztvevo> resztvevok = new List<Resztvevo>();
        Button dinamikusGomb;

        public MainWindow()
        {
            InitializeComponent();
            Beolvasas("VBfoci.csv");
            ComboBoxEv.ItemsSource = resztvevok.Select(r => r.Ev).Distinct().OrderBy(x => x);
            Valtoztatas();
        }
        
        private void Beolvasas(string fajlNev)  // Betölti a CSV fájlt és feltölti az adatszerkezetet
        {
            try
            {
                var sorok = File.ReadAllLines(fajlNev, Encoding.GetEncoding("iso-8859-2")).Skip(1);
                foreach (var sor in sorok)
                {
                    var mezok = sor.Split(';');
                    resztvevok.Add(new Resztvevo(mezok[0], int.Parse(mezok[1]), int.Parse(mezok[2]), mezok[3]));
                }
                ListBoxResztvevok.ItemsSource = resztvevok;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba a fájl beolvasásakor: " + ex.Message);
            }
        }

        private void Gomb_Click(object sender, RoutedEventArgs e)   // Megjeleníti az átlagos helyezést és a leggyakoribb helyszínt
        {
            double atlag = resztvevok.Average(r => r.Helyezes);
            string leggyakoribbHelyszin = resztvevok.GroupBy(r => r.Helyszin)
                                                    .OrderByDescending(g => g.Count())
                                                    .First().Key;

            MessageBox.Show($"Átlagos helyezés: {atlag:F2}\nLeggyakoribb helyszín: {leggyakoribbHelyszin}");
        }

        private void SzuresMentessel(string orszag)   // Szűri az ország alapján az adatokat és menti fájlba
        {
            var szurt = resztvevok.Where(r => r.Orszag.ToLower().Contains(orszag.ToLower())).ToList();

            if (szurt.Count == 0)
            {
                MessageBox.Show("Nincs találat.");
                return;
            }

            ListBoxResztvevok.ItemsSource = szurt;
            string fajl = "szurt.txt";
            var sorok = szurt.Select(r => $"{r.Orszag};{r.Helyezes};{r.Ev};{r.Helyszin}");
            File.WriteAllLines(fajl, sorok, Encoding.UTF8);
            MessageBox.Show($"Eredmények fájlba mentve: {fajl}");
        }

        private void SzuresGomb_Click(object sender, RoutedEventArgs e)   // Kezeli a szűrés és mentés gomb kattintását (ország mező alapján)
        {
            string orszag = TextBoxOrszag.Text.Trim();
            if (!string.IsNullOrWhiteSpace(orszag))
            {
                SzuresMentessel(orszag);
            }
            else
            {
                MessageBox.Show("Kérlek, adj meg egy országnevet!");
            }
        }

        private void ComboBoxEv_SelectionChanged(object sender, SelectionChangedEventArgs e)     // Év kiválasztásának eseménykezelője
        {
            if (ComboBoxEv.SelectedItem != null)
            {
                SzuresRendezes();
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)     // Checkbox események: csak döntősök bekapcsolása/kikapcsolása
        {
            SzuresRendezes();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)   
        {
            SzuresRendezes();
        }

        private void RadioButton_Ev_Checked(object sender, RoutedEventArgs e)   // Rendezési opciók eseményei
        {
            SzuresRendezes();
        }

        private void RadioButton_Helyezes_Checked(object sender, RoutedEventArgs e)
        {
            SzuresRendezes();
        }

        private void SzuresRendezes()    // Közös szűrési és rendezési logika az év, helyezés és döntő szűrőkhöz
        {
            IEnumerable<Resztvevo> lista = resztvevok;

            if (ComboBoxEv.SelectedItem != null)
            {
                int ev = (int)ComboBoxEv.SelectedItem;
                lista = lista.Where(r => r.Ev == ev);
            }

            if (CheckBoxDontosok.IsChecked == true)
            {
                lista = lista.Where(r => r.Helyezes <= 2);
            }

            if (RadioEv.IsChecked == true)
            {
                lista = lista.OrderBy(r => r.Ev);
            }
            else if (RadioHelyezes.IsChecked == true)
            {
                lista = lista.OrderBy(r => r.Helyezes);
            }

            ListBoxResztvevok.ItemsSource = lista.ToList();
        }

        private void Valtoztatas()    // Dinamikus gomb létrehozása és esemény: minden vezérlő alaphelyzetbe állítása
        {
            dinamikusGomb = new Button
            {
                Content = "Lista visszaállítása",
                Width = 120,
                Margin = new Thickness(10),
                Background = System.Windows.Media.Brushes.LightGray,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            dinamikusGomb.Click += (s, e) =>
            {
                ListBoxResztvevok.ItemsSource = resztvevok;
                ComboBoxEv.SelectedIndex = -1;
                CheckBoxDontosok.IsChecked = false;
                RadioEv.IsChecked = false;
                RadioHelyezes.IsChecked = false;
            };

            if (this.Content is Grid grid)
            {
                foreach (var elem in grid.Children)
                {
                    if (elem is StackPanel sp && Grid.GetRow(sp) == 2)
                    {
                        sp.Children.Add(dinamikusGomb);
                        break;
                    }
                }
            }
        }
    }

    public class Resztvevo    // Adatosztály: egy résztvevő VB-szereplésének tárolására
    {
        public string Orszag { get; set; }
        public int Helyezes { get; set; }
        public int Ev { get; set; }
        public string Helyszin { get; set; }

        public Resztvevo(string orszag, int helyezes, int ev, string helyszin)
        {
            Orszag = orszag;
            Helyezes = helyezes;
            Ev = ev;
            Helyszin = helyszin;
        }

        public override string ToString()
        {
            return $"{Ev} - {Orszag} ({Helyezes}. hely, {Helyszin})";
        }
    }
}
