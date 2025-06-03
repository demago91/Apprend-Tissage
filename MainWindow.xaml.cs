using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.Json;
using System.ComponentModel;
using Apprend_Tissage.AppClasses;
using System.Globalization;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace Apprend_Tissage
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool IsAppLoaded = false;

        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        private Services.ServiceLogs _serviceLogs;
        private Preferences _appPrefs;

        private readonly string FileLogs = "logs.json";
        private readonly string FilePrefs = "prefs.json";
        private readonly string FilePhrases = "phrases.json";
        private readonly string FileTextes = "textes.json";

        AppTextes _textes = new();

        Phrase? _phrase = null;

        ObservableCollection<Phrase> Phrases = [];

        private Clavier _clavier;

        // private RadioButton _radioButton;
        // private List<char> alphabet = [];

        private char? ToucheAttendue = null;
        private int KeyIndex = 0;

        Stopwatch _CalcTime = new Stopwatch();

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            PreviewKeyDown += MainWindow_PreviewKeyDown;
            PreviewKeyUp += MainWindow_PreviewKeyUp;
            LocationChanged += MainWindow_LocationChanged;
            SizeChanged += MainWindow_SizeChanged;

            
        }

        private void SauverPrefs()
        {
            string infos = JsonSerializer.Serialize<Preferences>(_appPrefs);
            File.WriteAllText(FilePrefs, infos);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(FilePrefs))
            {
                string infos = File.ReadAllText(FilePrefs);
                _appPrefs = JsonSerializer.Deserialize<Preferences>(infos);
            }
            else
            {
                _appPrefs = new();
                _appPrefs.SetPosition(Left, Top);
                _appPrefs.SetTaille(Width, Height);
            }

            if(!_appPrefs.claviers.Any()) {
                DefinirClaviers();
            }

            SetAccents(spAccentsMaj, "ÀÄÈÉÊËÎÏÔÖÛÙÜ");
            SetAccents(spAccentsMin, "àäèéêëîïôöûùü");

            _serviceLogs = new();
            _serviceLogs.Definir(FileLogs);

            bdRes.SetDefault();
            ucCapLock.SetDefault("MAJUSCULE");
            // ucNumLock.SetDefault("NUMLOCK");

            MainWindow_PreviewKeyDown(null, null);

            ChargerPhrases();

            string NL = Environment.NewLine;

            // Définie les textes des controles

            _textes.Ajouter("descZoneApprend", $"Bonjour, bienvenue dans l'application Apprend-Tissage, apprends à taper les mots sur le clavier.{NL}Sélectionne les mots depuis l'onglet a coté, ou c'est écrit 'Mots et Phrases'.");
            descZoneApprend.Text = _textes.Get("descZoneApprend");

            _textes.Ajouter("tooltip_texte", $"Appuie sur la touche 'entrée'{NL}dès que tu as réussie à écrire le mot correctement.");
            tbTexte.ToolTip = _textes.Get("tooltip_texte");

            var cboxOui = new CboxItem("Oui", 1);
            var cboxNon = new CboxItem("Non", 0);

            filtreReussi.Items.Add(cboxOui);
            filtreReussi.Items.Add(cboxNon);

            AfficherPhrases();
            AfficherScores();

            Left = _appPrefs.PositionX;
            Top = _appPrefs.PositionY;
            Width = _appPrefs.TailleX;
            Height = _appPrefs.TailleY;

            IsAppLoaded = true;
        }

        private void SetAccents(StackPanel sp, string Accents)
        {
            foreach (char ac in Accents) {
                Button b = new();
                b.Content = ac;
                b.FontSize = 12;
                b.Width = 32;
                b.Margin = new Thickness(2,6,2,6);
                b.Click += bAjoutAccentMaj_Click;
                sp.Children.Add(b);
            }
        }

        private void DefinirClaviers()
        {
            Clavier claAlpha = new()
            {
                Id = "alpha",
                Nom = "Alphabétique",
                alphabet = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'],
                Separateurs = [11, 21]
            };

            _appPrefs.claviers.Add(claAlpha);

            Clavier claAze = new()
            {
                Id = "aze",
                Nom = "Azerty",
                alphabet = ['A', 'Z', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P', 'Q', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'W', 'X', 'C', 'V', 'B', 'N'],
                Separateurs = [11, 21]
            };

            _appPrefs.claviers.Add(claAze);

            Clavier claQwe = new()
            {
                Id = "qwe",
                Nom = "Qwerty",
                alphabet = ['Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P', 'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'Z', 'X', 'C', 'V', 'B', 'N', 'M'],
                Separateurs = [11, 20]
            };

            _appPrefs.claviers.Add(claQwe);

            SauverPrefs();
        }

        private void AfficherScores()
        {
            if (_appPrefs != null)
            {
                
                tbTempsC1.Text = $"{_appPrefs.claviers[0].Score.Seconds}.{_appPrefs.claviers[0].Score.Milliseconds}";
                tbTempsC2.Text = $"{_appPrefs.claviers[1].Score.Seconds}.{_appPrefs.claviers[1].Score.Milliseconds}";
                tbTempsC3.Text = $"{_appPrefs.claviers[2].Score.Seconds}.{_appPrefs.claviers[2].Score.Milliseconds}";

            }
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsAppLoaded)
            {
                _appPrefs.SetTaille(Width, Height);
                SauverPrefs();
            }
        }

        private void MainWindow_LocationChanged(object? sender, EventArgs e)
        {
            if (IsAppLoaded)
            {
                _appPrefs.SetPosition(Left, Top);
                SauverPrefs();
            }
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            // var isScrollLockToggled = Keyboard.IsKeyToggled(Key.Scroll);

            var isCapsLockToggled = Keyboard.IsKeyToggled(Key.CapsLock);
            Brush col = isCapsLockToggled ? Brushes.GreenYellow : Brushes.DarkGray;
            rb_maj.IsChecked = isCapsLockToggled;
            rb_min.IsChecked = !isCapsLockToggled;
            ucCapLock.Set(col);

            if (isCapsLockToggled)
            {
                spAccentsMin.Visibility = Visibility.Collapsed;
                spAccentsMaj.Visibility = Visibility.Visible;
                
            } else
            {
                spAccentsMin.Visibility = Visibility.Visible;
                spAccentsMaj.Visibility = Visibility.Collapsed;
            }

            if(e != null && e.Key == Key.CapsLock && _phrase != null && _phrase.IsFill)
                SetPhrase(_phrase);

            var isNumLockToggled = Keyboard.IsKeyToggled(Key.NumLock);
            Brush colnum = isNumLockToggled ? Brushes.GreenYellow : Brushes.DarkGray;
            ucNumLock.Set(colnum);

            if(tcMain.SelectedIndex == 2)
                keyboard_KeyDown(sender, e);

            if (tcMain.SelectedIndex == 3)
                touche_KeyDown(sender, e);

        }

        private void MainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (tcMain.SelectedIndex == 2)
                keyboard_KeyUp(sender, e);

            if (tcMain.SelectedIndex == 3)
                touche_KeyUp(sender, e);
        }

        private void AfficherPhrases()
        {
            var phrases = FiltrerPhrases();

            tiPhrases.Header = $" - Mots et Phrases : {phrases.Count} - ";

            List<Phrase> list = new();

            if(filtreCat.SelectedValue != null && filtreCat.SelectedValue.ToString().Equals("chiffres"))
            {
                var dic = new Dictionary<int, Phrase>();
                foreach(var un in phrases) {
                    string[] nfo = un.Texte.Split(' ');
                    int.TryParse(nfo[0], out int no);

                    dic.Add(no, un);
                }
                
                foreach(var i in dic.OrderBy(x => x.Key)) {
                    list.Add(i.Value);
                }

                
            } else
            {
                list = phrases;
            }

            lvPhrases.ItemsSource = null;
            lvPhrases.ItemsSource = list;

            List<string> ctx = [.. Phrases.Where(x => x.ContexteEcriture != null).Select(x => x.ContexteEcriture).Distinct().OrderBy(x => x)];

            tb_addctx.ItemsSource = ctx;
            filtreCat.ItemsSource = ctx;

            miCtx.Items.Clear();
            foreach(var e in ctx) {
                MenuItem mi = new MenuItem();
                mi.Header = e.ToString();
                mi.Click += Mi_Click;

                miCtx.Items.Add(mi);
            }
        }

        private void Mi_Click(object sender, RoutedEventArgs e)
        {
            var eltmi = sender as MenuItem;
            string ctx = eltmi.Header.ToString();

            if (lvPhrases.SelectedItems.Count > 0)
            {
                var q = MessageBox.Show($"Modifier la catégorie des {lvPhrases.SelectedItems.Count} phrases en '{ctx}' ?", "Confirmation de modification", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (q == MessageBoxResult.Yes)
                {
                    foreach (Phrase ph in lvPhrases.SelectedItems)
                    {
                        ph.ContexteEcriture = ctx;

                        _serviceLogs.Logger("UPDATE CAT", $"changement de catégorie pour {ph.Texte} en '{ctx}'");
                    }

                    SauverPhrases();
                    AfficherPhrases();
                }
            }
        }

        private void tbTexte_KeyUp(object sender, KeyEventArgs e)
        {
            if(_phrase != null) {
                string ent = tbTexte.Text;

                if (!string.IsNullOrEmpty(ent))
                {

                    bool ok = false;

                    if (rb_mm.IsChecked.Value)
                    {
                        ok = _phrase.Texte.Equals(ent);
                    }
                    else if (rb_maj.IsChecked.Value)
                    {
                        ok = _phrase.Texte.ToUpper().Equals(ent);
                    }
                    else if (rb_min.IsChecked.Value)
                    {
                        ok = _phrase.Texte.ToLower().Equals(ent);
                    }

                    if (ok)
                    {
                        btSuiv.IsEnabled = true;

                        bdRes.SetOk();

                        _phrase.Reussie = 1;
                        Phrases.First(p => p.Texte == _phrase.Texte).Reussie = 1;

                        tblTexte.Background = Brushes.PaleGreen;

                        _serviceLogs.Logger("REUSSITE", $"Phrase correctement tapée pour {_phrase.Texte}");

                        AfficherPhrases();

                        SauverPhrases();

                        if (e.Key == Key.Enter)
                        {
                            PhraseSuivante();
                        }
                    }
                    else
                    {
                        bdRes.SetNotOk();
                    }
                } else
                {
                    bdRes.SetDefault();
                }
            }
        }

        private void rb_click(object sender, RoutedEventArgs e)
        {
            if (_phrase != null && _phrase.IsFill)
            {
                if (rb_mm.IsChecked.Value)
                {
                    tblTexte.Text = _phrase.Texte;
                }
                else if (rb_maj.IsChecked.Value)
                {
                    tblTexte.Text = _phrase.Texte.ToUpper();
                }
                else if (rb_min.IsChecked.Value)
                {
                    tblTexte.Text = _phrase.Texte.ToLower();
                }

                tbTexte.Clear();
                bdRes.SetDefault();
            }
        }

        private void btn_addphrase_click(object sender, RoutedEventArgs e)
        {
            AjouterPhrase();
        }

        private void AjouterPhrase()
        {
            
            string phrase = tb_addphrase.Text.Trim();

            string ctx = "";
            if (!string.IsNullOrEmpty(tb_addctx.Text))
            {
                ctx = tb_addctx.Text.Trim();
            }

            if (phrase.Length > 0) {

                bool IsOp = false;

                if (!Phrases.Any(p => p.Texte == phrase)) {

                    Phrase ph = new();
                    ph.Texte = tb_addphrase.Text;

                    if (ctx.Length > 0)
                    {
                        ph.ContexteEcriture = ctx;
                    }

                    _serviceLogs.Logger("AJOUT", $"Phrase ajoutée {ph.Texte}, {ph.ContexteEcriture}");

                    Phrases.Add(ph);
                    IsOp = true;
                }
                else
                {
                    if (Phrases.Any(p => p.Texte == phrase && p.ContexteEcriture == ctx))
                    {
                        MessageBox.Show("Cette phrase existe déjà dans la liste !");

                    } else
                    {
                        Phrase ph = Phrases.First(x => x.Texte == phrase);
                        ph.ContexteEcriture = ctx;

                        _serviceLogs.Logger("MODIF", $"Phrase modifié {ph.Texte}, {ph.ContexteEcriture}");

                        int phi = Phrases.IndexOf(ph);
                        Phrases[phi] = ph;
                        IsOp = true;
                    }
                }

                if (IsOp) {
                    var ord_phrases = Phrases.OrderBy(p => p.Texte).ToList();
                    Phrases = new(ord_phrases);
                    SauverPhrases();
                    AfficherPhrases();
                }

                tb_addphrase.Clear();
                
                if(!cbKeepCat.IsChecked.Value)
                    tb_addctx.Text = "";

                tb_addphrase.Focus();
            }
        }

        private void ChargerPhrases()
        {
            if(File.Exists(FilePhrases))
            {
                string str = File.ReadAllText(FilePhrases);
                List<Phrase> phs = JsonSerializer.Deserialize<List<Phrase>>(str);
                if (phs.Any())
                {
                    Phrases = new(phs);

                    tb_addctx.ItemsSource = phs.Select(x => x.ContexteEcriture).Distinct().ToList().OrderBy(x => x);
                }

            }
        }

        private void SauverPhrases()
        {
            string txt = JsonSerializer.Serialize(Phrases);
            File.WriteAllText("phrases.json", txt);
        }

        private void lvPhrases_DClick(object sender, MouseButtonEventArgs e)
        {
            Selectionner();
        }

        private void Selectionner()
        {
            if (lvPhrases.SelectedItem is Phrase ph)
            {
                SetPhrase(ph);
            }
        }

        private void SetPhrase(Phrase ph)
        {
            _phrase = ph;

            btSuiv.IsEnabled = false;

            tblTexte.Background = Brushes.AliceBlue;

            // cbTxtOk.Text = ph.Reussie == 1 ? "Déjà réussi" : "";
            bdRes.SetDefault();

            string sph = "";

            if (rb_mm.IsChecked.Value)
            {
                sph = _phrase.Texte;
            }
            else if (rb_maj.IsChecked.Value)
            {
                sph = _phrase.Texte.ToUpper();
            }
            else if (rb_min.IsChecked.Value)
            {
                sph = _phrase.Texte.ToLower();
            }

            tblTexte.Text = sph;
            tbCtx.Text = _phrase.ContexteEcriture;

            if (_phrase.ContexteEcriture != null)
            {
                if (_phrase.ContexteEcriture.Equals("couleur"))
                {
                    Dictionary<string, Brush> map = new Dictionary<string, Brush>
                    {
                        { "noir", Brushes.Black },
                        { "blanc", Brushes.White },
                        { "gris", Brushes.Gray },
                        { "jaune", Brushes.Yellow },
                        { "orange", Brushes.Orange },
                        { "rouge", Brushes.Red },
                        { "violet", Brushes.Violet },
                        { "marron", Brushes.Maroon },
                        { "bleu", Brushes.Blue },
                        { "rose", Brushes.MistyRose },
                        { "vert", Brushes.Green }
                    };

                    if (map.ContainsKey(_phrase.Texte))
                    {
                        recColor.Fill = map[_phrase.Texte];
                    }
                } else
                {
                    string dir = Directory.GetCurrentDirectory();
                    string pic = $"{dir}\\pictos\\" + _phrase.Texte + ".png";
                    if (File.Exists(pic))
                    {
                        ImageBrush imgBrush = new ImageBrush();
                        imgBrush.ImageSource = new BitmapImage(new Uri(pic));

                        recColor.Fill = imgBrush;
                    } else
                    {
                        recColor.Fill = null;
                    }
                }
            }
            else
            {
                recColor.Fill = Brushes.White;
            }

            tbTexte.Clear();

            tiApp.IsSelected = true;
            tbTexte.Focus();

        }

        private void btn_alphabet_click(object sender, RoutedEventArgs e)
        {
            string str = "";
            for (int i = 0; i < 26; i++) {
                char c = (char)(i+97);
                str += c;
            }

            if(!Phrases.Any(p => p.Texte == str)) {
                Phrase ph = new();
                ph.Texte = str;
                Phrases.Add(ph);
                SauverPhrases();
            } else
            {
                MessageBox.Show("L'alphabet existe déjà dans la liste !", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void tb_addphrase_keydown(object sender, KeyEventArgs e)
        {
            if (!string.IsNullOrEmpty(tb_addphrase.Text.Trim()))
            {
                btn_addphrase.IsEnabled = true;

                if (e.Key == Key.Enter)
                {
                    AjouterPhrase();

                    // tb_addphrase.Clear();
                    // tb_addctx.Text = "";
                }

                
            }
            else
            {
                btn_addphrase.IsEnabled = false;
            }
            
        }

        private void lvPhrases_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                Supprimer();
            }
        }

        private void btn_eff_reussite_click(object sender, RoutedEventArgs e)
        {
            SupprimerReussites();
        }

        private void SupprimerReussites()
        {
            var msg = MessageBox.Show("Effacer les réussites", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (msg == MessageBoxResult.Yes)
            {
                foreach (var item in Phrases)
                {
                    item.Reussie = 0;
                }

                _serviceLogs.Logger("SUPPRESSION", $"Réussites effacées");

                SauverPhrases();
                AfficherPhrases();

                resultats.Text = "";
                btSupprimeReussite.IsEnabled = false;
            }
        }

        private void lvPhrasesColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                lvPhrases.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            lvPhrases.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void tblTexte_MouseDown(object sender, MouseButtonEventArgs e)
        {
            tiPhrases.Focus();
            lvPhrases.Focus();
        }

        private void btSuiv_Click(object sender, RoutedEventArgs e)
        {
            PhraseSuivante();
        }

        private void PhraseSuivante()
        {
            int max = lvPhrases.Items.Count;
            int suiv = lvPhrases.Items.IndexOf(_phrase);
            if (suiv > -1)
            {
                suiv++;
                if (suiv < max)
                {

                    Phrase ph = (Phrase)lvPhrases.Items[suiv];
                    SetPhrase(ph);
                }

            }

            VerifierTout();
        }

        private void VerifierTout()
        {
            bool all = true;
            foreach (Phrase item in lvPhrases.Items)
            {
                if(item.Reussie == 0)
                {
                    all = false;
                    break;
                }
                    
            }

            if (all)
            {
                resultats.Text = "BRAVO, tu as réussie a tout taper correctement !";

                _serviceLogs.Logger("SUCCES", resultats.Text);

                btSupprimeReussite.IsEnabled = true;
            } else
            {
                resultats.Text = "Il te reste des phrases a taper !";
            }
        }

        private void btSupprimeReussite_Click(object sender, RoutedEventArgs e)
        {
            SupprimerReussites();
        }

        private void miMod_Click(object sender, RoutedEventArgs e)
        {
            if(lvPhrases.SelectedItem is Phrase ph)
            {
                expEdit.IsExpanded = true;
                tb_addphrase.Text = ph.Texte;
                tb_addctx.Text = ph.ContexteEcriture;
            }
        }

        private void miSel_Click(object sender, RoutedEventArgs e)
        {
            Selectionner();
        }

        private void miSup_Click(object sender, RoutedEventArgs e)
        {
            Supprimer();
        }

        private void Supprimer()
        {
            if (lvPhrases.SelectedItem is Phrase ph)
            {
                var msg = MessageBox.Show($"Effacer la Phrase '{ph.Texte}' ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (msg == MessageBoxResult.Yes)
                {
                    _serviceLogs.Logger("SUPPRESSION", $"Phrase supprimée {ph.Texte}, {ph.ContexteEcriture}");

                    Phrases.Remove(ph);
                    SauverPhrases();
                    AfficherPhrases();
                }
            }
        }

        private void zoneAjMod_OpenClose(object sender, RoutedEventArgs e)
        {
            if (sender is Expander exp)
            {
                double hauteur = exp.IsExpanded ? 30 : 0;

                Thickness margin = new(10, 10+hauteur, 10, 10);

                lvPhrases.Margin = margin;
            }
        }

        private void btn_import_click(object sender, RoutedEventArgs e)
        {
            ImporterPhrases();
        }

        private void btn_export_click(object sender, RoutedEventArgs e)
        {
            ExporterListe();
        }

        private void ImporterPhrases()
        {
            throw new NotImplementedException();
        }
        

        private void ExporterListe()
        {
            throw new NotImplementedException();
        }

        private void cbFiltreCat_SelChanged(object sender, SelectionChangedEventArgs e)
        {
            AfficherPhrases();
        }

        private List<Phrase> FiltrerPhrases()
        {
            var res = Phrases.ToList();

            // if (filtreTexte.Text.Length > 0 || filtreCat.SelectedValue != null || filtreReussi.SelectedItem != null || filtreAccent.IsChecked != null || filtrePhrase.IsChecked != null) {
                // res = Phrases.ToList();

                if (filtreTexte.Text.Length > 0)
                {
                    res = Phrases.ToList().FindAll(x => x.Texte.Contains(filtreTexte.Text));
                }

                if (filtreCat.SelectedValue != null)
                {
                    string fc = filtreCat.SelectedValue.ToString();
                    res = res.FindAll(x => x.ContexteEcriture == fc);
                }

                if (filtreReussi.SelectedValue != null && filtreReussi.SelectedValue is CboxItem ci)
                {
                    res = res.FindAll(x => x.Reussie == ci.Value);
                }

                if (filtreAccent.IsChecked != null)
                {
                    bool acc = filtreAccent.IsChecked.Value;
                    res = res.FindAll(x => HasDiacritics(x.Texte) == acc );
                } 

                if (filtrePhrase.IsChecked.Value)
                {
                    res = res.FindAll(x => x.Texte.Contains(" "));
                }

                resultats.Text = "";

/*            } else {
                res = Phrases.ToList();
            } */

            return res;
        }

        public static bool HasDiacritics(string value)
        {
            if (value == null) return false;

            var normalize = value.Normalize(NormalizationForm.FormD);

            var sb = new StringBuilder();

            foreach (var t in normalize.Where(t => CharUnicodeInfo.GetUnicodeCategory(t) != UnicodeCategory.NonSpacingMark))
                sb.Append(t);

            return (sb.ToString() != value);
        }

        private void tbFiltreText_TextChanged(object sender, TextChangedEventArgs e)
        {
            AfficherPhrases();
        }

        private void bFiltreClear_Click(object sender, RoutedEventArgs e)
        {
            filtreTexte.Clear();
            filtreCat.SelectedItem = null;
            filtreReussi.SelectedItem = null;
            filtreAccent.IsChecked = false;
            filtrePhrase.IsChecked = false;
        }

        private void cbFiltreReussi_SelChanged(object sender, SelectionChangedEventArgs e)
        {
            AfficherPhrases();
        }

        private void cbFiltreAccent_SelChanged(object sender, RoutedEventArgs e)
        {
            // CheckBox_Content(sender as CheckBox);
            AfficherPhrases();
        }

        private void CheckBox_Content(CheckBox cb)
        {
            cb.Content = cb.IsChecked.Value ? "Oui" : "";
        }

        private void cbFiltrePhrase_SelChanged(object sender, RoutedEventArgs e)
        {
            // CheckBox_Content(sender as CheckBox);
            AfficherPhrases();
        }

        private void tiClavier_GotFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void Clavier_Check(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb)
            {
                _clavier = _appPrefs.claviers.First(x => x.Id == rb.Name);
                // _radioButton = rb;
                AfficherClavier();
            }
        }

        private void AfficherClavier()
        {
            // var rb = _radioButton;
            KeyIndex = 0;
            wpKeys.Children.Clear();
            
            double dx = 10;
            double dy = 10;

            double esp = 50;

            int index = 1;

            int ind_y = 0;

            foreach (var ac in _clavier.alphabet)
            {

                string cl = ac.ToString().ToUpper();
                Button b = new()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Padding = new Thickness(2),
                    Width = 40,
                    Height = 40,
                    Content = cl,
                    Name = "KEY_" + cl,
                    Background = Brushes.White,
                    FontFamily = new FontFamily("Cascadia Mono"),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                };

                Thickness margin = new(dx, dy, 0, 0);
                b.Margin = margin;

                wpKeys.Children.Add(b);

                index++;
                if (_clavier.Separateurs.Contains(index))
                {
                    ind_y++;
                    dx = ind_y*20;
                    dy += esp;
                }
                else
                {
                    dx += esp;
                }
            }

            
            AfficherTouche();
        }

        private void AfficherTouche()
        {
            ToucheAttendue = _clavier.alphabet[KeyIndex];
            tbToucheAtt.Text = ToucheAttendue.ToString();
        }

        private void ResetClavier()
        {
            KeyIndex = 0;
            foreach (Button b in wpKeys.Children)
            {
                b.Background = Brushes.White;
            }

            AfficherTouche();
        }

        private void keyboard_KeyDown(object sender, KeyEventArgs e)
        {
            if (e != null)
            {
                string touche = e.Key.ToString();
                var key = e.Key;
                string keyval = "KEY_" + key.ToString().ToUpper();

                foreach (Button b in wpKeys.Children)
                {
                    if (b.Name.Equals(keyval))
                    {
                        // b.Background = Brushes.Blue;

                        if(ToucheAttendue != null && ToucheAttendue.ToString() == touche)
                        {
                            b.Background = Brushes.Green;
                            KeyIndex++;

                            if(KeyIndex == 1)
                            {
                                _CalcTime.Start();
                            }

                            if (KeyIndex < _clavier.alphabet.Count)
                            {
                                AfficherTouche();
                            }
                            else if (KeyIndex == _clavier.alphabet.Count)
                            {
                                _CalcTime.Stop();

                                TimeSpan temps = _CalcTime.Elapsed;

                                string win = "";

                                
                                if(_clavier.Score.Seconds == 0 || temps < _clavier.Score)
                                {
                                    _clavier.Score = _CalcTime.Elapsed;
                                    _appPrefs.SetScore(_clavier, temps);
                                    win = "\r\n Tu as battu ton meilleur score.";
                                    SauverPrefs();
                                }
                                
                                /*
                                if (_radioButton.Name == "aze")
                                {
                                    if (_appPrefs.Temps_C2.Seconds == 0 || temps < _appPrefs.Temps_C2)
                                    {
                                        _appPrefs.Temps_C2 = _CalcTime.Elapsed;
                                        win = "\r\n Tu as battu ton meilleur score.";
                                        SauverPrefs();
                                    }
                                }


                                if (_radioButton.Name == "qwe")
                                {
                                    if (_appPrefs.Temps_C3.Seconds == 0 || temps < _appPrefs.Temps_C3)
                                    {
                                        _appPrefs.Temps_C3 = _CalcTime.Elapsed;
                                        win = "\r\n Tu as battu ton meilleur score.";
                                        SauverPrefs();
                                    }
                                }
                                */

                                if (!string.IsNullOrEmpty(win))
                                {
                                    AfficherScores();
                                }

                                MessageBox.Show($"BRAVO, tu as tapé sur toute les touches sans te tromper !\r\n Tu l'as fait en {_CalcTime.Elapsed.TotalSeconds}.{win}");
                                _CalcTime.Reset();
                            }
                        } else {
                            ResetClavier();


                        }
                        
                    }
                }
            }
        }

        private void keyboard_KeyUp(object sender, KeyEventArgs e)
        {
            /*
            if (e != null)
            {
                var key = e.Key;
                string keyval = "KEY_" + key.ToString().ToUpper();

                foreach (Button b in wpKeys.Children)
                {
                    if (b.Name.Equals(keyval))
                    {
                        b.Background = Brushes.White;
                    }
                }
            }
            */
        }

        private void touche_KeyDown(object sender, KeyEventArgs e)
        {
            if (e != null)
            {
                var key = e.Key;
                string keyval = "KEY_" + key.ToString().ToUpper();

                foreach (var b in wpTouches.Children)
                {
                    if(b is Button btn)
                    {
                        TB_KEY.Text = key.ToString();
                        if (btn.Name.Equals(keyval))
                        {
                            btn.Background = Brushes.Blue;
                        }
                    }
                        
                }
            }
        }

        private void touche_KeyUp(object sender, KeyEventArgs e)
        {
            if (e != null)
            {
                var key = e.Key;
                string keyval = "KEY_" + key.ToString().ToUpper();

                foreach (var b in wpTouches.Children)
                {
                    if (b is Button btn)
                    {
                        if (btn.Name.Equals(keyval))
                        {
                            btn.Background = Brushes.White;
                        }
                    }
                }
            }
        }

        private void tiTouches_GotFocus(object sender, RoutedEventArgs e)
        {
            if (wpTouches.Children.Count == 0)
            {
                AfficherTouches();
            }
        }

        private void AfficherTouches()
        {
            // wpTouches.Children.Clear();

            string[] touches = ["F1","F2","F3","F4","F5","F6","F7","F8","F9","F10","F11","F12",
                                "OEMQUOTES", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "D0", "BACK",
                                "A", "Z", "E", "R", "T", "Y", "U", "I", "O", "P", "RETURN",
                                "Q", "S", "D", "F", "G", "H", "J", "K", "L",
                                "W", "X", "C", "V", "B", "N", "UP", "SPACE", "LEFT", "DOWN", "RIGHT",
                                "INSERT", "DELETE", "HOME", "END"];  

            int nb = touches.Length -7;
            int[] indexes = [13, 25, 36, 46, nb];


            double dx = 10;
            double dy = 10;

            int index = 1;
            int ind_y = 0;
            
            double esp = 50;

            foreach (var ac in touches)
            {

                string cl = ac.ToString().ToUpper();
                string ctn = cl;

                if (cl == "<") cl = "PP";
                if (cl == ">") cl = "PG";
                if (cl == "UP") ctn = "🠕";
                if (cl == "LEFT") ctn = "🠔";
                if (cl == "DOWN") ctn = "🠗";
                if (cl == "RIGHT") ctn = "🠖";

                Button b = new()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Padding = new Thickness(2),
                    Width = 40,
                    Height = 40,
                    Content = ctn,
                    Name = "KEY_" + cl,
                    Background = Brushes.White,
                    FontFamily = new FontFamily("Cascadia Mono"),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                };

                b.Click += buttonClick;
                Thickness margin = new Thickness(dx, dy, 0, 0);


                if (cl == "RETURN") { b.Height = 90; b.Width = 70; margin.Left += 61; }
                if (cl == "BACK") { b.Width = 90; margin.Left += 12; }
                if (cl == "SPACE") b.Width = 400;

                if (cl == "UP") margin.Left = 590;
                if (cl == "LEFT") margin.Left = 538;
                if (cl == "DOWN") margin.Left = 590;
                if (cl == "RIGHT") margin.Left = 640;

                if (cl == "INSERT") { b.HorizontalAlignment = HorizontalAlignment.Right;  margin.Top = 10; margin.Right = 50; b.Width = 80; }
                if (cl == "DELETE") { b.HorizontalAlignment = HorizontalAlignment.Right; margin.Top = 60; margin.Right = 50; b.Width = 80; }
                if (cl == "HOME") { b.HorizontalAlignment = HorizontalAlignment.Right; margin.Top = 110; margin.Right = 50; b.Width = 80; }
                if (cl == "END") { b.HorizontalAlignment = HorizontalAlignment.Right; margin.Top = 160; margin.Right = 50; b.Width = 80; }

                b.Margin = margin;
                b.ToolTip = margin;
                wpTouches.Children.Add(b);

                index++;
                if (indexes.Contains(index)) {
                    ind_y++;
                    dx = ind_y*20;
                    dy += esp;
                } else {
                    dx += esp;
                }
            }

            Thickness mg = new(10, dy+64, 0, 0);
            TextBox textBox = new()
            {
                Width = 600,
                Margin = mg,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Padding = new Thickness(4),
                Background = Brushes.White,
                FontFamily = new FontFamily("Cascadia Mono"),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Name = "tbKeys", 
            };


            wpTouches.Children.Add(textBox);

        }

        private void buttonClick(object sender, RoutedEventArgs e)
        {
            
        }

        private void bAjoutAccentMaj_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content.ToString().Length > 0) {

                tbTexte.Text += btn.Content;
                tbTexte.SelectionStart = tbTexte.Text.Length;
                tbTexte.SelectionLength = 0;
                tbTexte.Focus();
            }
        }


        private void cbFiltreAcc_Click(object sender, RoutedEventArgs e)
        {
            AfficherPhrases();
        }
    }
}