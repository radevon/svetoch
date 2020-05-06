var unicom = angular.module("unicom", []);

unicom.controller('UnicomController', ['$scope','$interval','requests',function UnicomController($scope, $interval, requests) {
    $scope.EWdata = {
        startDate: new Date(),
        endDate: new Date(),
        data: []
    };


    
    // посчитаный расход воды по табличным данным
    $scope.WaterRate = function () {
        var filtered = $scope.EWdata.data.filter(function (e, i) { // фильтрую данные по >0 импульсов
            return e.TotalWaterRate > 0;
        });
        // расход беру по вычисленному значению
        if (filtered.length > 0) {
            return filtered[filtered.length - 1].WaterCalculated-filtered[0].WaterCalculated;
        } else {
            return 0;
        }
    };

    $scope.EnergyRate = function () {
        if ($scope.EWdata.data.length > 0) {
            return $scope.EWdata.data[$scope.EWdata.data.length - 1].TotalEnergy-$scope.EWdata.data[0].TotalEnergy;
        } else {
            return 0;
        }
    };

    // время в минутах
    $scope.intervalData = function () {
        if ($scope.EWdata.data.length > 0) {
            return ($scope.EWdata.data[$scope.EWdata.data.length - 1].RecvDate-$scope.EWdata.data[0].RecvDate) / 1000 / 60;
        } else {
            return 0;
        }
    };

    
    $scope.init = function (identity) {
        $scope.loadData(identity);
        
    };

    // запрос данных
    $scope.loadData = function (identity,dateStart) {
        showLoading(true);
        requests.getLastData(identity, dateStart)
            .success(function (data) {
                $scope.EWdata.startDate = new Date(data.StartDate);
                $scope.EWdata.endDate = new Date(data.EndDate);

                $scope.EWdata.data = [];
                data.DataTable.map(function (e, i) {
                    $scope.EWdata.data.push({
                        Id: e.Id,
                        RecvDate: new Date(e.dt),
                        TotalEnergy: e.enrg,
                        Amperage1: e.a1,
                        Amperage2: e.a2,
                        Amperage3: e.a3,
                        Voltage1: e.u1,
                        Voltage2: e.u2,
                        Voltage3: e.u3,
                        CurrentElectricPower: e.pw,
                        TotalWaterRate: e.wtr,
                        WaterEnergy: e.en2wt,
                        Frequrency: e.freq,
                        Presure: e.pres,
                        Temperature: e.temp,
                        WaterKoef: e.koef,
                        WaterStartValue: e.wtr0,
                        WaterCalculated: e.wtrc,
                        Errors: e.err,
                        EngineAmp:e.enA
                    });
                });
                
            }).error(function (error) { console.log(error); })
            .finally(function () {
                showLoading(false);
            });
    };


    $scope.getClassRowError = function (elem) {
        if (elem != null && elem.length > 0) {
            return 'error_field';
        }
        return '';
    };

}]);


unicom.service("requests", ['$http',function ($http) {
    this.getLastData = function (identity,date_start) {
        return $http({
            url: '../GetByPeriod',
            method: 'GET',
            dataType: 'json',
            cache: false,
            params: { 'identity': identity, 'parameterGraph': 'Amperage1', 'start_': date_start.toISOString() }
        });
    };
}]);
