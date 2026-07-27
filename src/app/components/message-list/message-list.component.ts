import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MessageService } from '../../services/message.service';

@Component({
  selector: 'app-message-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './message-list.component.html',
  styleUrls: ['./message-list.component.css']
})
export class MessageListComponent {
  private readonly messageService = inject(MessageService);

  readonly messages$ = this.messageService.messages$;

  dismiss(id: number): void {
    this.messageService.remove(id);
  }
}
