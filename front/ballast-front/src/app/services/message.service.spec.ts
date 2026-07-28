import { beforeEach, describe, expect, it } from 'vitest';
import { MessageService } from './message.service';

describe('MessageService', () => {
  let service: MessageService;

  beforeEach(() => {
    service = new MessageService();
  });

  it('should add a message to the stream', () => {
    let latestMessages: Array<{ id: number; text: string; type: 'success' | 'error' | 'info' }> = [];
    service.messages$.subscribe((messages) => {
      latestMessages = messages;
    });

    service.add('Saved successfully', 'success');

    expect(latestMessages).toEqual([
      { id: 1, text: 'Saved successfully', type: 'success' }
    ]);
  });

  it('should remove and clear messages', () => {
    let latestMessages: Array<{ id: number; text: string; type: 'success' | 'error' | 'info' }> = [];
    service.messages$.subscribe((messages) => {
      latestMessages = messages;
    });

    service.add('First', 'info');
    service.add('Second', 'error');
    service.remove(1);

    expect(latestMessages).toEqual([
      { id: 2, text: 'Second', type: 'error' }
    ]);

    service.clear();
    expect(latestMessages).toEqual([]);
  });
});
