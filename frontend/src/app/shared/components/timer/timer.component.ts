import { Component, computed, inject } from '@angular/core';
import { NgClass } from '@angular/common';
import { RealtimeService } from '@app/services/realtime.service';


@Component({
  selector: 'app-timer',
  standalone: true,
  imports: [NgClass],
  templateUrl: './timer.html',
  styleUrls: ['./timer.css']
})
export class TimerComponent {
  rt = inject(RealtimeService);
  display = computed(() => {
    const s = this.rt.timeoutRunning() ? this.rt.timeoutLeft() : this.rt.timeLeft();
    const m = Math.floor(s / 60);
    const r = s % 10
    return `${m.toString().padStart(2,'0')}:${r.toString().padStart(2,'0')}`;
  });
}
