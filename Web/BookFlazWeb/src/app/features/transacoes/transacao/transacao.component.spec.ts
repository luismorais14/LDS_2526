import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TransacaoComponent } from './transacao.component';
import { TransacaoService } from '../../../services/transacao.service';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';

describe('TransacaoComponent', () => {
  let component: TransacaoComponent;
  let fixture: ComponentFixture<TransacaoComponent>;

  beforeEach(async () => {
    const transacaoServiceMock = jasmine.createSpyObj('TransacaoService', [
      'obterPorId',
      'cancelarTransacao',
      'confirmarRececaoComprador',
      'registarDevolucao',
      'confirmarDevolucao'
    ]);
    
    transacaoServiceMock.obterPorId.and.returnValue(of({}));

    const activatedRouteMock = {
      snapshot: {
        paramMap: {
          get: () => '1' 
        }
      }
    };

    await TestBed.configureTestingModule({
      imports: [TransacaoComponent],
      providers: [
        { provide: TransacaoService, useValue: transacaoServiceMock },
        { provide: ActivatedRoute, useValue: activatedRouteMock }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TransacaoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});