import { Component, OnInit, Input, EventEmitter, Output } from '@angular/core';
import { Photo } from 'src/app/_models/Photo';
import { AdminService } from 'src/app/_services/admin.service';
import { AlertifyService } from 'src/app/_services/alertify.service';

@Component({
  selector: 'app-photo-card',
  templateUrl: './photo-card.component.html',
  styleUrls: ['./photo-card.component.css']
})
export class PhotoCardComponent implements OnInit {
  @Input() photo: Photo;
  @Output() photoProcessed = new EventEmitter();

  constructor(private adminService: AdminService, private alertify: AlertifyService) { }

  ngOnInit() {
  }

  approve() {
    this.adminService.approvePhoto(this.photo.id).subscribe(() => {
      this.photoProcessed.emit(this.photo);
      this.alertify.success('Photo has been approved');
    }, error => {
      this.alertify.error(error);
    });
  }
  reject() {
    this.adminService.rejectPhoto(this.photo.id).subscribe(() => {
      this.photoProcessed.emit(this.photo);
      this.alertify.success('Photo has been rejected');
    }, error => {
      this.alertify.error(error);
    });
  }
}
