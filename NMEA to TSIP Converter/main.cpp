/*
 * NEMA to TSIP Converter.cpp
 * ATmega328P 16 MHz
 * Created: 28.06.2017 7:30:19
 * Author : Andrey
 */ 

#include "stdafx.h"
#include "HardwareUART.cpp"
#include "SoftwareUART.cpp"
#include "NmeaParser.cpp"


#define SUART_RX_PIN 7
#define SUART_TX_PIN 6
#define SUART_RX_PORT PIND
#define SUART_TX_PORT PORTD
#define INT1_PIN _BV(3)
#define INT1_PORT PORTD

SoftUart nmeaUart = SoftUart(SUART_RX_PORT,SUART_RX_PIN,SUART_TX_PORT,SUART_TX_PIN);
HardUart tsipUart = HardUart(9600, ParityAndStop::Odd1);
RingBuffer<120> nmeaBuffer = RingBuffer<120>();
//inline void TsipPushRaw(u8 data)
//{
	//tsipBuffer.Push(data);
//}
NmeaParser parser = NmeaParser(tsipUart);
volatile u16 timeCounter;
u8 timePrescaler;

ISR(TIMER1_CAPT_vect) //9600*3
{
	if(++timePrescaler >= 29)
	{
		timePrescaler = 0; 
		++timeCounter;
		++parser.ppsTimeMSec;
	}
	if(EIFR & _BV(INTF1)) //����������� �����
	{
		++parser.ppsTimeSec;
		parser.ppsTimeMSec = 0;
		EIFR &= _BV(INTF1);
	}
	if(INT1_PORT & INT1_PIN)
	{
		parser.ppsTimeMSec = 0;
	}
	
	u8 data;
	if(nmeaUart.RxProcessing(data))
	{
		nmeaBuffer.Push(data);
	}
}

void mainLoop()
{
	if(nmeaBuffer.Size())
	{
		cli();
		u8 c = nmeaBuffer.Pop();
		sei();
		//tsipBuffer.Push(tmp);
		if(timeCounter>5000 && parser.ppsTimeMSec > 1 && parser.ppsTimeMSec < 500) 
		{
			timeCounter = 0;
			parser.isHealthSend = true;
		}
		parser.Parse(c);
		//if(timeCounter > 1)
		//{
			//tsipBuffer.Push(0xAA);
			//tsipBuffer.Push(timeCounter>>8);
			//tsipBuffer.Push(timeCounter);
		//}
		//if(nmeaBuffer.isOverflow || tsipBuffer.isOverflow)
		//{
			//tsipBuffer.Push(0xEE);
			//tsipBuffer.Push(0x0F);
			//if(nmeaBuffer.isOverflow) tsipBuffer.Push('N');
			//if(tsipBuffer.isOverflow) tsipBuffer.Push('T');
			//nmeaBuffer.isOverflow = 0;
			//tsipBuffer.isOverflow = 0;
		//}
	}
}




int main()
{
	// Declare your local variables here
	
	// Crystal Oscillator division factor: 1
	clock_prescale_set(clock_div_1);

	// Input/Output Ports initialization
	PORTB=0x00;
	DDRB = LED_PIN;

	PORTC=0x00;
	DDRC=0x00;

	PORTD=0x00;
	DDRD=0x00;

  //PORTD=_BV(SUART_TX_PORT);
  //DDRD=_BV(SUART_TX_PORT);

  // Timer/Counter 0 initialization
  // Clock source: System Clock
  // Clock value: Timer 0 Stopped
  // Mode: Normal top=0xFF
  // OC0A output: Disconnected
  // OC0B output: Disconnected
  TCCR0A=0x00;
  TCCR0B=0x00;
  TCNT0=0x00;
  OCR0A=0x00;
  OCR0B=0x00;

  // Timer/Counter 1 initialization
  // Clock source: System Clock
  // Clock value: 16000,000 kHz
  // Mode: CTC top=ICR1
  // OC1A output: Discon.
  // OC1B output: Discon.
  // Noise Canceler: Off
  // Input Capture on Falling Edge
  // Timer1 Overflow Interrupt: Off
  // Input Capture Interrupt: On
  // Compare A Match Interrupt: Off
  // Compare B Match Interrupt: Off
  TCCR1A=0x00;
  TCCR1B=0x19;
  TCNT1H=0x00;
  TCNT1L=0x00;
  ICR1H=0x02;
  ICR1L=0x2B;
  OCR1AH=0x00;
  OCR1AL=0x00;
  OCR1BH=0x00;
  OCR1BL=0x00;

  // Timer/Counter 2 initialization
  // Clock source: System Clock
  // Clock value: Timer2 Stopped
  // Mode: Normal top=0xFF
  // OC2A output: Disconnected
  // OC2B output: Disconnected
  ASSR=0x00;
  TCCR2A=0x00;
  TCCR2B=0x00;
  TCNT2=0x00;
  OCR2A=0x00;
  OCR2B=0x00;

 // External Interrupt(s) initialization
 // INT0: Off
 // INT1: On
 // INT1 Mode: Rising Edge
 // Interrupt on any change on pins PCINT0-7: Off
 // Interrupt on any change on pins PCINT8-14: Off
 // Interrupt on any change on pins PCINT16-23: Off
 EICRA=0x0C;
 EIMSK=0x00;
 EIFR=0x02; //INTF1: External Interrupt Flag 1
 PCICR=0x00;

  // Timer/Counter 0 Interrupt(s) initialization
  TIMSK0=0x00;

  // Timer/Counter 1 Interrupt(s) initialization
  TIMSK1=0x20;

  // Timer/Counter 2 Interrupt(s) initialization
  TIMSK2=0x00;

  // Analog Comparator initialization
  // Analog Comparator: Off
  // Analog Comparator Input Capture by Timer/Counter 1: Off
  ACSR=0x80;
  ADCSRB=0x00;
  DIDR1=0x00;

  // ADC initialization
  // ADC disabled
  ADCSRA=0x00;

  // SPI initialization
  // SPI disabled
  SPCR=0x00;

  // TWI initialization
  // TWI disabled
  TWCR=0x00;

  // Watchdog Timer initialization
  // Watchdog Timer Prescaler: OSC/16k
  // Watchdog Timer interrupt: Off
  wdt_enable(WDTO_120MS);

  // Global enable interrupts
  sei();

	static const u8 softwareVersion[15] PROGMEM = {0x10, 0x45, 0x01, 0x10, 0x10, 0x02, 0x02, 0x06, 0x02, 0x19, 0x0C, 0x02, 0x05, 0x10, 0x03};
	for(u8 i=0; i<15; i++)
	{
		tsipUart.TransmitAndWait(pgm_read_byte(&softwareVersion[i]));
		wdt_reset();
	}

  while (1)
  {
    mainLoop();
	wdt_reset();
  }
  return 0;
}
