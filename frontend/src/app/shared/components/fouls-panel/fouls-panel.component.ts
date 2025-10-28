import { CommonModule } from '@angular/common';
import { Component, Input, computed, inject } from '@angular/core';
import { RealtimeService } from '@app/services/realtime.service';


@Component({
  selector: 'app-fouls-panel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './fouls-panel.component.html',
  styleUrls: ['./fouls-panel.component.css']
})
export class FoulsPanelComponent {
  private rt = inject(RealtimeService);

  /** Indica qué lado mostrar */
  @Input() side: 'home' | 'away' = 'home';

  /** Valor reactivo de faltas según el lado */
  value = computed(() =>
    this.side === 'home' ? this.rt.fouls().home : this.rt.fouls().away
  );
}
