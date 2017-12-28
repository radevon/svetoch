var app = angular.module("app", []);



app.controller('MarkerController',function MarkerController($scope,$http,dataService)
{
    
    $scope.init = function () {
        $scope.loadMarkers();
    };

    $scope.markers = [];
    $scope.flagEdit = false;
    $scope.mapView = true;
    $scope.selectedMarker = null;

    $scope.loadMarkers = function () {
        dataService.getmarkers()
            .success(function (data) {
                $scope.markers = data;
            })
            .error(function (error) { console.log(error); }); 
    };
    
    $scope.$watch('mapView', function (newValue, oldValue) {
        if (newValue == true) {

            $scope.scrollToCenter();
        }
    });

    // метод вставки записи метки в базу 
    $scope.insertMarker = function (marker) {
        // вставка через метод сервиса
        dataService.insertmarker(marker)
        .success(function (id) {
            // в случае успеха получить объект из базы
            dataService.getmarkerbyid(id)
            .success(function (data) {
                // добавляем в массив
                $scope.markers.push(data);
            })
            .error(function (error) { console.log(error); });
        })
        .error(function (error) { console.log(error); });
    };

    $scope.updateMarker = function (marker) {
        // вставка через метод сервиса
        dataService.updatemarker(marker)
        .success(function (updated) {
            // если обновлено > 0 записей
            if (updated>0)
            // получить обновленный объект из базы
             dataService.getmarkerbyid(marker.MarkerId)
             .success(function (data) {
                // обновляем маркер в массиве
                for (var i = 0; i < $scope.markers.length; i++) {
                    if ($scope.markers[i].MarkerId == data.MarkerId)
                        $scope.markers[i] = angular.copy(data);
                }
             })
             .error(function (error) { console.log(error); });
        })
        .error(function (error) { console.log(error); });
    };

    $scope.deleteMarker = function (MarkerId) {
        dataService.deletemarker(MarkerId)
        .success(function (deleted) {
            // обновляем маркер в массиве
            if (deleted > 0)
            {
                // обновляем маркер в массиве
                for (var i = 0; i < $scope.markers.length; i++) {
                    if ($scope.markers[i].MarkerId == MarkerId) {
                        var index = $scope.markers.indexOf($scope.markers[i]);
                        $scope.markers.splice(index, 1);
                    }
                }
                        
            }
        })
             .error(function (error) { console.log(error); });
    };

		

	$scope.initMarker=function(){
	$scope.editMarker={
		 	MarkerId:0,
		 	Address: '',
            MarkerType:0,
			Identity:'',
			Px:null,
			Py:null
		 };
		};

	$scope.add=function(){
			$scope.initMarker();
			$scope.selectedMarker = null;
	        $scope.mapView = true;
			$scope.flagEdit = true;
			$scope.searchCriteria = null;
		};

	$scope.edit=function(){
			$scope.editMarker=angular.copy($scope.selectedMarker);
			$scope.selectedMarker=null;
			$scope.flagEdit = true;
			$scope.mapView = true;
		};

	$scope.details=function(){

			if($scope.selectedMarker!=null) {

			    window.location.href = '././Details/ViewParameters/' + $scope.selectedMarker.MarkerId;

			}
			
		};

	$scope.cancelAdd=function(){
			$scope.flagEdit=false;
			$scope.initMarker();
			$scope.selectedMarker=null;
		};

    $scope.scrollToCenter = function() {
        var height_container = $('#mapcontainer').height(),
        width_container = $('#mapcontainer').width();


        var top = 0;
        var left = 0;
        // берем координаты куда надо скролить (в зависимости от состояния выбранного маркера)
        if ($scope.flagEdit == true) {
            top = $scope.editMarker.Py;
            left = $scope.editMarker.Px;
        } else {
            if ($scope.selectedMarker != null) {
                top = $scope.selectedMarker.Py;
                left = $scope.selectedMarker.Px;
            } else {
                top = $('#map').height() / 2;
                left = $('#map').width() / 2;
            }
        }

        $('#mapcontainer').animate({
                scrollTop: top - height_container / 2.0,
                scrollLeft: left - width_container / 2.0
            }, 500);
       
    };

		// функция обработки нажатия на маркер
	$scope.select=function(marker,event){
			if($scope.flagEdit==false)
				{
					$scope.selectedMarker=marker;
					$scope.searchCriteria=null;
				}

			// прекращаем всплытие события к родителю
			if (event.stopPropagation) event.stopPropagation();
		    if (event.preventDefault) event.preventDefault();
		    event.cancelBubble = true;
		    event.returnValue = false;

		};

		// функция обработки нажатия на пустую область на карте
	$scope.mapClick=function(event){
			if($scope.flagEdit==false)
				$scope.selectedMarker=null;
			else
			{
				var target=event.target;
				if(target.id==="map")
				{
					var offset=target.getBoundingClientRect();
					var x=offset.left;
					var y=offset.top;
					var absx=event.pageX-x-14;
					var absy=event.pageY-y-16;

					$scope.editMarker.Px=+absx.toFixed(5);
					$scope.editMarker.Py=+absy.toFixed(5);
				}

				
			}
		};


    $scope.deleteMark = function(marker) {
        if (confirm('Вы подтверждаете удаление информации?')) {
            $scope.deleteMarker(marker.MarkerId);
            $scope.selectedMarker = null;
        }
        ;
    };

		// логика добавления - редактирования
	$scope.submitEditForm=function(){
			if ($scope.editform.$valid) {

				if($scope.editMarker.MarkerId==0) // вставка
				{
				    $scope.insertMarker($scope.editMarker);
				}
                else  // обновление
				{
				    $scope.updateMarker($scope.editMarker);
                    
                }
                
                $scope.flagEdit=false;
			    $scope.initMarker();
			    $scope.selectedMarker=null;

            }else
            {
            	alert('Не все данные указаны верно!');
            }
		};

    $scope.search = function(term) {
        var array = [];
        var pattern = new RegExp($.trim(term), 'i');
        angular.forEach($scope.markers, function(value, key) {
            if (pattern.test(value.Address))
                this.push(value);
        }, array);
        return array;
    };

});