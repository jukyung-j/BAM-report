using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BAM
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Size = new Size(1000, 500);
            // PictureBox1 속성
            pictureBox1.Location = new Point(68, 63);
            pictureBox1.Size = new Size(256, 256);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            // PictureBox2 속성
            pictureBox2.Location = new Point(647, 63);
            pictureBox2.Size = new Size(256, 256);
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.BorderStyle = BorderStyle.FixedSingle;
            // PictureBox3 속성
            pictureBox3.Location = new Point(359,192);
            pictureBox3.Size = new Size(256, 256);
            pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox3.BorderStyle = BorderStyle.FixedSingle;
        }
        public byte[] BitmapToByteArray2D(Bitmap bmp)
        {
            byte[] bmpArray = new byte[bmp.Height * bmp.Width];
            int brightness;
            Color gray;
            for (int x = 0; x < bmp.Width; x++)
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color pixelColor = bmp.GetPixel(x, y);
                    brightness = (int)(0.299 * pixelColor.R + 0.587 * pixelColor.G + 0.114 * pixelColor.B); // gray 변환
                    gray = Color.FromArgb(brightness, brightness, brightness);
                    bmp.SetPixel(x, y, gray);
                    pixelColor = bmp.GetPixel(x, y);
                    bmpArray[y * bmp.Width + x] = pixelColor.R;
                }
            return bmpArray;
        }
        private int[] hopfieldSetWeight(int[] image,int[] image2, int height, int width)
        {
            int[,] S = new int[height * width,2];
            int[] W = new int[height * width];
            int[] result = { 1, -1 };    // 아이유 1, 로제 -1 결과값

            for (int i = 0; i < height*width; i++)
            {
                S[i,0] = image[i];
                S[i,1] = image2[i];

            }
            for (int n = 0; n < 2; n++) {
                for (int i = 0; i < height * width; i++)
                {
                    W[i] += S[i,n] * result[n];
                }
            }
            return W;
        }
        public int hopfieldPredict(int[] image,int[] W, int height, int width)
        {
            int[] Y = new int[width * height];
            int[] X = new int[width * height];
            int[,] S = new int[width * height, 2];
            int result = 0 ;

            for (int i = 0; i < height*width; i++)
            {
                 Y[i] = image[i];
            }
            int[] new_Y = new int[height * width];

            for(int z = 0; z < 2; z++) {
                int net = 0;
                for (int j = 0; j < height * width; j++)
                {
                    net += W[j] * Y[j];
                }
                int new_y = 0;
                if (net < 0)
                {
                    new_y = -1;
                }
                else if (net > 0)
                {
                    new_y = 1;
                }
                else
                {
                    new_y = 0;      // 다른 사람
                    result = new_y;
                    break;
                }
                for (int j = 0; j < height * width; j++)
                {
                    X[j] = W[j] * new_y;
                    if (X[j] > 0)
                        X[j] = 1;
                    else if (X[j] < 0)
                        X[j] = -1;
                    else
                        X[j] = Y[j];
                    Y[j] = X[j];
                }
                result = new_y;
            }
           
            return result;
    }
       
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();     // 클래스 객체 생성 직접 코딩, 속도 메모리공간 효율
            open.Filter = "Image Files(*.jpg; *.jpeg; *.png; *.bmp)|*.jpg; *.jpeg; *.png; *.bmp";
            open.Title = "학습할 이미지 선택";
            if (open.ShowDialog() == DialogResult.OK)
            {
                // 화면에 보이기
                pictureBox1.Image = new Bitmap(open.FileName);  // Bitmap 객체 생성 imagebox에 집어넣기
            }
            open = new OpenFileDialog();
            open.Filter = "Image Files(*.jpg; *.jpeg; *.png; *.bmp)|*.jpg; *.jpeg; *.png; *.bmp";
            open.Title = "학습할 이미지 선택";
            if (open.ShowDialog() == DialogResult.OK)
            {
                // 화면에 보이기
                pictureBox2.Image = new Bitmap(open.FileName);  // Bitmap 객체 생성 imagebox에 집어넣기
            }

        }

        private void bAMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Title = "예측할 이미지 선택";
            open.Filter = "Image Files(*.jpg; *.jpeg; *.png; *.bmp)|*.jpg; *.jpeg; *.png; *.bmp";
            if (open.ShowDialog() == DialogResult.OK)
            {
                pictureBox3.Image = new Bitmap(open.FileName);
            }

            Bitmap img = (Bitmap)pictureBox1.Image;
            Bitmap img2 = (Bitmap)pictureBox2.Image;
            Bitmap test_img3 = (Bitmap)pictureBox3.Image;
            int height = img.Height;
            int width = img.Width;

            byte[] imgArray = BitmapToByteArray2D(img);    // r,g,b 1Byte씩이라서 메모리공간을 효율적으로 하기 위해
            byte[] imgArray2 = BitmapToByteArray2D(img2);
            byte[] imgArray3 = BitmapToByteArray2D(test_img3);

            // 1. 학습 이미지 초기화(이진화 포함)
            int[] learn_img = new int[height*width];
            int[] learn_img2 = new int[height*width];
            for (int i = 0; i < height*width; i++)
            { 
                learn_img[i] = imgArray[i] > 128 ? 1 : -1;
                learn_img2[i] = imgArray2[i] > 128 ? 1 : -1;
             }

            int[] W = hopfieldSetWeight(learn_img, learn_img2, height, width);

            int[] target_img = new int[height*width];

            for (int i = 0; i < height*width; i++) 
                 target_img[i] = imgArray3[i] > 128 ? 1 : -1;

           
            // 4. 학습된 결과 이미지 예측
            int result;

            result = hopfieldPredict(target_img, W, height, width);

            if (result == 1)
                MessageBox.Show("아이유 입니다");
            else if (result == -1)
                MessageBox.Show("로제입니다");
            else
                MessageBox.Show("error");

        }
    }
}
