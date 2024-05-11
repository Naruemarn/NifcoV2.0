
/*
  Software serial multple serial test

 Receives from the two software serial ports,
 sends to the hardware serial port.*/


#include <SoftwareSerial.h>
#include <avr/wdt.h>

// software serial #1: RX = digital pin 10, TX = digital pin 11
SoftwareSerial Serial_Machine(10, 11);   // PIN10 RX <-- Machine
// Serial = Hardware Serail , PIN0 RX <--- Server , PIN1 TX --> To Sever
//-----------------------------------------------------------------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------------------------------------------------------------------
#define ledPin 13
#define EOT 0x04
#define CR  0x0D
//-----------------------------------------------------------------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------------------------------------------------------------------
const int bufferSize = 500;
char inputBuffer[bufferSize];
int bufferPointer = 0;
int i = 0, sec = 0, minutes = 0, hours = 0;

String id = "6";
String myString;
String reSponse;
bool f_ready = false;
//-----------------------------------------------------------------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------------------------------------------------------------------
void setup() 
{
    wdt_enable(WDTO_4S);// สั่งให้เริ่มจับเวลา 4 วินาที ถ้าเกินให้ Reset โปรแกรมใหม่
  
    /*// Timer
    DDRB |= B00100000;  // set pin13 to output without affecting other pins
    
    noInterrupts();         
    TCCR1A = 0;
    TCCR1B = 0;
    TCNT1  = 0;
  
    OCR1A = 6250;            // 0.1 sec.
    TCCR1B |= (1 << WGM12);   // CTC mode
    TCCR1B |= (1 << CS12);    // 256 prescaler
    TIMSK1 |= (1 << OCIE1A);  // enable timer compare interrupt
    interrupts();             // enable all interrupts*/
    
    // Open serial communications and wait for port to open:
    Serial.begin(38400); // To PC
    
    
    
    while (!Serial) 
    {
      ; // wait for serial port to connect. Needed for native USB port only
    }
     
      Serial_Machine.begin(38400); // TO Machine
    //Serial_Machine.setTimeout(3000);

    //Serial.println("Reset");
}
//-----------------------------------------------------------------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------------------------------------------------------------------
/*ISR(TIMER1_COMPA_vect)          // timer1 Interrupt ทุกๆ 0.1 วินาที
{
  i++;

  if(i >= 1)
  {
    PORTB ^= B00100000;// toggles bit which affects pin13  OS LED
    i = 0;
  }
  
  /*if(i >= 10)
  {
    sec++;
    i = 0;
  }
  if(sec >= 60)
  {
    minutes++;
    sec = 0;
  } 
  if(minutes >= 60)
  {
    hours++;
    minutes = 0;
  }tyg
}*/

//-----------------------------------------------------------------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------------------------------------------------------------------
/*void get_data_from_machine____TEST()
{
  int incomingByte = 0;
  char inByte;
  int count = 0;
   
  
   if(Serial_Machine.available() > 0) 
   {
      inByte = Serial_Machine.read();
      
      if (inByte == EOT) 
      {

        for (int i=0; i < bufferPointer; i++)    // HDR HDR .... EOT
        {
          if (inputBuffer[i] == 'H') count++;
        }

             
        //Serial.println("Count HDR : " + String(count));

        if(count == 1)
        {
          // 'EOT' character
          inputBuffer[bufferPointer++] = EOT;
  
          
          if((inputBuffer[0] == 'H') && (inputBuffer[1] == 'R') && (inputBuffer[2] == 'D'))              
          {
            //delay(200);
            Serial.print(inputBuffer);
          }
          else
          {
              //Serial.println("Error Packet");
          }    

          bufferPointer = 0;
          count = 0;
          memset(inputBuffer, 0, sizeof(inputBuffer));
          //Serial.println("END");
        }
        else
        {
          bufferPointer = 0;
          count = 0;
          memset(inputBuffer, 0, sizeof(inputBuffer));
          //Serial.println("HRD > 1 per packet");
        }
      }
      else
      {          
          // not a 'EOT' character
          if (bufferPointer < bufferSize - 1)  // Leave room for a null terminator
          {
              inputBuffer[bufferPointer++] = inByte;  

              if(inputBuffer[0] == 'H')
              {
                //Serial.println("HDR");
              }  
              else
              {
                bufferPointer = 0;
                memset(inputBuffer, 0, sizeof(inputBuffer));
                Serial.println("Clear");
              } 
          } 
          else
          {
            bufferPointer = 0;
            memset(inputBuffer, 0, sizeof(inputBuffer));
            Serial.println("Buffer > Sizeof");                       
          }
      }      
   }
}*/
//-----------------------------------------------------------------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------------------------------------------------------------------
void delay2(int d)
{
  for(i=0; i<=d; i++)
  {
    if((Serial.available() > 0) || (Serial_Machine.available() > 0))
    {
      f_ready = false;
      reSponse="";
      myString="";   
      break;
    }
    else
    {
      delay(1);
    }
    
  }
}
//-----------------------------------------------------------------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------------------------------------------------------------------
void get_data_from_machine()
{
  if(!f_ready)
  {
    if(Serial_Machine.available() > 0)
    {
      
      char recieved = Serial_Machine.read();
      myString += recieved; 
      
      if (recieved == EOT)
      {        
        myString = "START," + id + "," + myString;
        f_ready =true; 
            
        /*String Header = myString.substring(0, 3);
        if(Header == "HRD")
        {
          f_ready =true;             
        }
        else
        {
          myString="";
        }*/
      }        
    }
  }
  else
  {
    Serial.println(myString);  
    long randNumber = random(2500,3500); // 2500-3499
    delay2(randNumber);

    if(Serial.available() > 0)
    {    
      reSponse = Serial.readStringUntil('\n');
      if(reSponse == id)
      {
        //Serial.println("STOP");
        f_ready = false;
        reSponse="";
        myString="";      
      }
        
    }
  }
    
  
}
//-----------------------------------------------------------------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------------------------------------------------------------------
void loop() 
{
    get_data_from_machine();
    wdt_reset(); //Reset การจับเวลาใหม่
  
}
