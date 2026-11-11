using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace KasimPatisi
{
    public partial class Form1 : Form
    {
        // Ekranı sürekli yenileyen zamanlayıcı ve geçen süreyi ölçen saat
        private readonly System.Windows.Forms.Timer zamanlayici = new System.Windows.Forms.Timer();
        private readonly Stopwatch saat = Stopwatch.StartNew();

        // Buketi çizerken kullandığımız referans pencere boyutu
        private const float TasarimGenislik = 700f;
        private const float TasarimYukseklik = 800f;

        // Sapların birleştiği nokta (buketin alt kısmı)
        private readonly Point demetNoktasi = new Point(350, 620);

        // Her çiçeğin ortasının konumu
        private readonly Point[] cicekMerkezleri =
        {
            new Point(350, 200),
            new Point(235, 290),
            new Point(465, 290),
            new Point(300, 385),
            new Point(400, 385),
            new Point(350, 300)
        };

        // Her çiçeğin rengi
        private readonly Color[] cicekRenkleri =
        {
            Color.FromArgb(255, 200, 60),
            Color.FromArgb(240, 100, 160),
            Color.FromArgb(245, 245, 250),
            Color.FromArgb(190, 90, 220),
            Color.FromArgb(255, 140, 50),
            Color.FromArgb(255, 90, 90)
        };

        // Animasyon süreleri (saniye)
        private const double CicekAraligi = 1.3;   // İki çiçek arasındaki bekleme
        private const double SapSuresi = 0.8;      // Sapın büyüme süresi
        private const double AcilmaSuresi = 2.0;   // Çiçeğin açılma süresi

        public Form1()
        {
            Text = "Kasım Patı Buketi";
            ClientSize = new Size(700, 800);
            BackColor = Color.Black;
            DoubleBuffered = true;
            ResizeRedraw = true;
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;

            // Boşluk tuşuna basılınca animasyon baştan başlar
            KeyDown += (gonderen, olay) =>
            {
                if (olay.KeyCode == Keys.Space)
                {
                    saat.Restart();
                }
            };

            // Her 16 milisaniyede bir ekranı yenile
            zamanlayici.Interval = 16;
            zamanlayici.Tick += (gonderen, olay) => Invalidate();
            zamanlayici.Start();
        }

        protected override void OnPaint(PaintEventArgs olay)
        {
            base.OnPaint(olay);
            Graphics cizici = olay.Graphics;
            cizici.SmoothingMode = SmoothingMode.AntiAlias;
            double gecenSure = saat.Elapsed.TotalSeconds;

            // Pencere boyutuna göre ölçekle ve ortala
            float olcek = Math.Min(ClientSize.Width / TasarimGenislik, ClientSize.Height / TasarimYukseklik);
            if (olcek <= 0) return;
            float kaydirX = (ClientSize.Width - TasarimGenislik * olcek) / 2f;
            float kaydirY = (ClientSize.Height - TasarimYukseklik * olcek) / 2f;
            cizici.TranslateTransform(kaydirX, kaydirY);
            cizici.ScaleTransform(olcek, olcek);

            // Sapları çiz (alttan yukarı doğru uzarlar)
            for (int sira = 0; sira < cicekMerkezleri.Length; sira++)
            {
                double ilerleme = Math.Clamp((gecenSure - sira * CicekAraligi) / SapSuresi, 0, 1);
                if (ilerleme <= 0) continue;

                float ucX = demetNoktasi.X + (cicekMerkezleri[sira].X - demetNoktasi.X) * (float)ilerleme;
                float ucY = demetNoktasi.Y + (cicekMerkezleri[sira].Y - demetNoktasi.Y) * (float)ilerleme;
                using (Pen sap = new Pen(Color.FromArgb(70, 160, 70), 5))
                {
                    cizici.DrawLine(sap, demetNoktasi.X, demetNoktasi.Y, ucX, ucY);
                }
            }

            // Çiçekleri çiz (tek tek açılırlar)
            for (int sira = 0; sira < cicekMerkezleri.Length; sira++)
            {
                double acilma = Math.Clamp((gecenSure - sira * CicekAraligi - SapSuresi * 0.7) / AcilmaSuresi, 0, 1);
                if (acilma <= 0) continue;

                double yumusakAcilma = 1 - Math.Pow(1 - acilma, 3);
                CicekCiz(cizici, cicekMerkezleri[sira], cicekRenkleri[sira], yumusakAcilma);
            }

            // Ambalaj kâğıdı ve kurdele
            int saydamlik = (int)(255 * Math.Clamp(gecenSure / 1.0, 0, 1));
            Point[] ambalaj =
            {
                new Point(260, 560),
                new Point(440, 560),
                new Point(390, 740),
                new Point(310, 740)
            };
            using (Brush kagit = new SolidBrush(Color.FromArgb((int)(saydamlik * 0.9), 200, 150, 120)))
            {
                cizici.FillPolygon(kagit, ambalaj);
            }
            using (Brush kurdele = new SolidBrush(Color.FromArgb(saydamlik, 220, 40, 90)))
            {
                cizici.FillRectangle(kurdele, 305, 595, 90, 18);
                cizici.FillEllipse(kurdele, 318, 580, 28, 28);
                cizici.FillEllipse(kurdele, 354, 580, 28, 28);
            }

            // Üstteki yazı ve alttaki ipucu (çiçekler açıldıktan sonra belirir)
            double bitisZamani = (cicekMerkezleri.Length - 1) * CicekAraligi + SapSuresi + AcilmaSuresi;
            int yaziSaydamligi = (int)(255 * Math.Clamp((gecenSure - bitisZamani) / 1.5, 0, 1));
            if (yaziSaydamligi > 0)
            {
                using (Font buyukYazi = new Font("Segoe UI", 20, FontStyle.Italic))
                using (Font kucukYazi = new Font("Segoe UI", 10))
                using (Brush pembeFirca = new SolidBrush(Color.FromArgb(yaziSaydamligi, 255, 190, 210)))
                using (Brush grifirca = new SolidBrush(Color.FromArgb(yaziSaydamligi, 150, 150, 150)))
                using (StringFormat ortala = new StringFormat { Alignment = StringAlignment.Center })
                {
                    cizici.DrawString("Ne ilk Kasım, ne ilk Kasımpatı,", buyukYazi, pembeFirca, new RectangleF(0, 20, 700, 40), ortala);
                    cizici.DrawString("ne son Kasımımız, ne son Kasımpatın", buyukYazi, pembeFirca, new RectangleF(0, 58, 700, 40), ortala);
                    cizici.DrawString("Yeniden izlemek için Boşluk tuşuna bas", kucukYazi, grifirca, new RectangleF(0, 760, 700, 30), ortala);
                }
            }
        }

        // İki rengi verilen oranda karıştırır (0 = ilk renk, 1 = ikinci renk)
        private static Color Karistir(Color birinciRenk, Color ikinciRenk, double oran)
        {
            return Color.FromArgb(
                (int)(birinciRenk.R + (ikinciRenk.R - birinciRenk.R) * oran),
                (int)(birinciRenk.G + (ikinciRenk.G - birinciRenk.G) * oran),
                (int)(birinciRenk.B + (ikinciRenk.B - birinciRenk.B) * oran));
        }

        // Tek bir kasımpatı çiçeği çizer
        private void CicekCiz(Graphics cizici, Point merkez, Color renk, double acilma)
        {
            // Çiçeğin arkasındaki hafif ışık halesi
            float haleYaricapi = (float)(85 * acilma);
            using (Brush haleFircasi = new SolidBrush(Color.FromArgb(35, renk)))
            {
                cizici.FillEllipse(haleFircasi, merkez.X - haleYaricapi, merkez.Y - haleYaricapi, haleYaricapi * 2, haleYaricapi * 2);
            }

            // Üç katman taç yaprak: dıştan içe doğru
            int[] yaprakSayilari = { 24, 20, 16 };
            float[] yaprakUzunluklari = { 62, 46, 30 };

            for (int katman = 0; katman < 3; katman++)
            {
                double katmanAcilmasi = Math.Clamp((acilma - katman * 0.2) / 0.6, 0, 1);
                if (katmanAcilmasi <= 0) continue;

                float yaprakBoyu = (float)(yaprakUzunluklari[katman] * katmanAcilmasi);
                float donusAcisi = (float)((1 - katmanAcilmasi) * 40);
                Color katmanRengi = Karistir(renk, Color.White, katman * 0.25);
                int yaprakSayisi = yaprakSayilari[katman];

                using (Brush yaprakFircasi = new SolidBrush(Color.FromArgb((int)(255 * Math.Min(1, katmanAcilmasi * 2)), katmanRengi)))
                using (Pen kenarKalemi = new Pen(Color.FromArgb(90, 0, 0, 0), 1))
                {
                    for (int sira = 0; sira < yaprakSayisi; sira++)
                    {
                        float aci = sira * (360f / yaprakSayisi) + katman * 7.5f + donusAcisi;
                        GraphicsState eskiDurum = cizici.Save();
                        cizici.TranslateTransform(merkez.X, merkez.Y);
                        cizici.RotateTransform(aci);
                        cizici.FillEllipse(yaprakFircasi, -5, -yaprakBoyu, 10, yaprakBoyu);
                        cizici.DrawEllipse(kenarKalemi, -5, -yaprakBoyu, 10, yaprakBoyu);
                        cizici.Restore(eskiDurum);
                    }
                }
            }

            // Çiçeğin ortasındaki sarı daire
            float ortaYaricapi = (float)(11 * acilma);
            using (Brush ortaFircasi = new SolidBrush(Color.FromArgb(220, 160, 40)))
            {
                cizici.FillEllipse(ortaFircasi, merkez.X - ortaYaricapi, merkez.Y - ortaYaricapi, ortaYaricapi * 2, ortaYaricapi * 2);
            }
        }
    }
}