#define MD_L1 9 // left motor
#define MD_L2 10 // left motor
#define MD_R1 5 // right motor
#define MD_R2 6 // right motor
#define dist  4 //sensor slot 1

int distance = 0;
char serialBuff[32];
int bufferIndex = 0;
int runtime = 0;
int powl = 0;
int powl_for = 0;
int powl_back = 0;
int powr = 0;
int powr_for = 0;
int powr_back = 0;

long readDistance(int triggerPin, int echoPin)
{
  pinMode(triggerPin, OUTPUT);
  digitalWrite(triggerPin, LOW);
  delayMicroseconds(2);
  digitalWrite(triggerPin, HIGH);
  delayMicroseconds(10);
  digitalWrite(triggerPin, LOW);
  pinMode(echoPin, INPUT);
  return pulseIn(echoPin, HIGH);
}

void setup() {
  // put your setup code here, to run once:
  Serial.begin(115200);
  pinMode(MD_R1, OUTPUT);
  pinMode(MD_R2, OUTPUT);
  pinMode(MD_L1, OUTPUT);
  pinMode(MD_L2, OUTPUT);
  pinMode(dist, INPUT);
}

void loop() {

  delay(10);
  //Serial.println(runtime);

  if(runtime > 0){
    runtime--;
  }

  if(runtime > 0 ){
    analogWrite(MD_R1, powr_for);
    analogWrite(MD_R2, powr_back);
    analogWrite(MD_L1, powl_for);
    analogWrite(MD_L2, powl_back);
  } else {
    analogWrite(MD_R1, 0);
    analogWrite(MD_R2, 0);
    analogWrite(MD_L1, 0);
    analogWrite(MD_L2, 0);
  }
}



void serialEvent() {
  while (Serial.available()) {
    char c = Serial.read();
    serialBuff[bufferIndex] = c;
    if( c == '\n' || c == '\0') {
      serialBuff[bufferIndex] = '\0';
      String succ = serialBuff;
      int ind1 = succ.indexOf(',');
      runtime = succ.substring(0, ind1).toInt();
      int ind2 = succ.indexOf(',', ind1+1);
      powr = succ.substring(ind1+1, ind2).toInt();
      if(powr < 0){
        powr_for = 0;
        powr_back = powr;
      }
      else if (powr > 0){
        powr_for = powr;
        powr_back = 0;
      }
      else{
        powr_for = 0;
        powr_back = 0;
      }
      int ind3 = succ.indexOf(',', ind2+1);
      powl = succ.substring(ind2+1, ind3).toInt();
      if(powl < 0){
        powl_for = 0;
        powl_back = powl;
      }
      else if (powl > 0){
        powl_for = powl;
        powl_back = 0;
      }
      else{
        powl_for = 0;
        powl_back = 0;
      }
      bufferIndex = 0;
    }
    else {
      if (bufferIndex < 31){
        bufferIndex++;
      }
    }
  }
}