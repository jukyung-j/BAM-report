# BAM-report
## 1. 개발 배경

코로나19로 인해서 마스크 착용이 필수화되고 있다.  
항상 마스크를 착용하고 있어 아이폰의 경우 Face ID로 잠금을 해제하는 데 불편함을 느끼고 있다. 마스크를 착용하지 않은 얼굴을 등록하면 마스크를 착용한 얼굴을 인식해서 잠금이 해제되면 편리하겠다 생각하여 개발하게 되었다.


## 2. 프로그램 설명
(2-1) BAM이란?  
<img src="https://user-images.githubusercontent.com/68947314/101620172-90276a00-3a57-11eb-909c-1c0fb97cf88e.png" width="30%" height="40%"></img>











BAM이란, 2개의 층(X 입력층, Y 출력층)으로 구성되어있다.  
각 층은 완전히 연결된 구조를 이루고 있으며  


X → Y 가중치 행렬은<img src="https://user-images.githubusercontent.com/68947314/101620279-ad5c3880-3a57-11eb-9dc6-0bf6cc3c8509.png"></img>   이고,



Y → X 가중치 행렬은 위 행렬의 전치행렬이다.   


정리하여, BAM은 단층구조(X층, Y층)이고, 두 개의 층이 완전히 연결되어 있는 양방향 연상 메모리이다. 입력값과 출력값이 다른 이질 연상 메모리이며 양극성으로 데이터를 처리한다.  



### (2-2) 코드 설명


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

Bitmap 데이터를 Byte로 변환하는 메소드이다.  
이미지를 흑백으로 변환한뒤 byte로 값을 저장한다.  

        private int[] BamSetWeight(int[] image,int[] image2, int height, int width)
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

가중치를 계산하는 메소드이다.   
이때 결과값을 첫 번째 사진을 1, 두 번 째 사진을 –1로 설정한다.   

        public int BamPredict(int[] image,int[] W, int height, int width)
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
 // BAM에 저장할 수 있는 패턴수 min(x,y)  
 
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
                new_y = 0;      
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
           
            return result;
    }

가중치와 테스트 이미지를 이용해 결과값을 예측하는 메소드이다.   

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

                byte[] imgArray = BitmapToByteArray2D(img);    
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

                int[] W = BamSetWeight(learn_img, learn_img2, height, width);

                int[] target_img = new int[height*width];

                for (int i = 0; i < height*width; i++) 
                     target_img[i] = imgArray3[i] > 128 ? 1 : -1;


                // 2. 학습된 결과 이미지 예측
                int result;

                result = BamPredict(target_img, W, height, width);

                 if (result == 1)
                     MessageBox.Show("아이유 입니다");
                 else if (result == -1)
                     MessageBox.Show("로제입니다");
                 else
                     MessageBox.Show("error");

            }
        }
    }






## 3. 프로그램 동작

<img src="https://user-images.githubusercontent.com/68947314/101620344-c4028f80-3a57-11eb-8e99-b53ca9b08601.png"></img>  
시작화면으로 File 메뉴를 선택하여 왼쪽과 오른쪽 상자에 Train Data를 입력하고,  
BAM 메뉴를 선택하여 중간 상자에 Test Data를 입력한다.  


<img src="https://user-images.githubusercontent.com/68947314/101620391-d8468c80-3a57-11eb-9da5-e1a2483301eb.png"></img>  
Train Data를 입력한 화면이다.  
왼쪽 상자에는 가수 아이유를 입력하고, 오른쪽 상자에는 블랙핑크 로제를 입력하였다.  




<img src="https://user-images.githubusercontent.com/68947314/101620471-ec8a8980-3a57-11eb-8c8a-b3c3c45081e2.png"></img>  
<img src="https://user-images.githubusercontent.com/68947314/101620523-ff04c300-3a57-11eb-9c67-ea732e6fe866.png"></img>  



Test Data를 입력한 화면이다.  
위 그림에는 아이유의 마스크 착용한 사진을 입력하고, 아래 그림에는 로제의 마스크 착용한 사진을 입력하였다. 각각 그에 맞는 결과가 출력되었다.  




## 4. 개선 방향

로제의 경우 머리색이 밝은색이 아닌 어두운 색의 사진을 테스트하면 잘못된 결과가 나옵니다.   

개선방안은?  
좀 더 다양한 사진을 가중치에 학습시키면 보다 정확한 결과를 얻을 수 있을 것으로 생각합니다.  

