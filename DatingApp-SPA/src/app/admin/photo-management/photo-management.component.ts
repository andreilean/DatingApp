import { Component, OnInit } from '@angular/core';
import { Photo } from 'src/app/_models/Photo';
import { AdminService } from 'src/app/_services/admin.service';
import { AlertifyService } from 'src/app/_services/alertify.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  photos: Photo[];
  constructor(private adminService: AdminService, private alertify: AlertifyService) { }

  ngOnInit() {
    this.adminService.getPhotosForModeration().subscribe((photos: Photo[]) => {
        this.photos = photos;
    }, error => {
        this.alertify.error(error);
    });
  }

  removePhoto(photo) {
    this.photos.splice(this.photos.findIndex(p => p.id === photo.id), 1);
  }
}
