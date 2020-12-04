import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AudioService {

  private audioContext = new AudioContext();

  /**
   * Credit to https://odino.org/emit-a-beeping-sound-with-javascript/
   */
  public beep(volume: number, frequency: number, duration: number) {
    const oscillator = this.audioContext.createOscillator();
    const gain = this.audioContext.createGain();
    oscillator.connect(gain);
    oscillator.frequency.value = frequency;
    oscillator.type = 'square';
    gain.connect(this.audioContext.destination);
    gain.gain.value = volume * 0.01;
    oscillator.start(this.audioContext.currentTime);
    oscillator.stop(this.audioContext.currentTime + duration * 0.001);
  }
}
