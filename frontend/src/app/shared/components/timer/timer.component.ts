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
    const total = this.rt.timeoutRunning() ? this.rt.timeoutLeft() : this.rt.timeLeft();
    const safeTotal = Number.isFinite(total) && total >= 0 ? total : 0;
    const minutes = Math.floor(safeTotal / 60);
    const seconds = safeTotal % 60;
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  });
}
