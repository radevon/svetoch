
    var margin = { top: 30, right: 0, bottom: 30, left: 70 },
       width = 980,
       height = 300;

    var parseDate = d3.time.format("%X").parse;

    var x = d3.time.scale()
        .range([0, width]);

    var y = d3.scale.linear()
        .range([height, 0]);

    var xAxis = d3.svg.axis()
        .scale(x)
        .orient("bottom")
        .ticks(d3.time.minute, 5)
        .tickFormat(d3.time.format("%X"));
        

    var yAxis = d3.svg.axis()
        .scale(y)
        .orient("left")
        .innerTickSize(-width)
        .outerTickSize(0)
        .tickPadding(10);

    var line = d3.svg.line()
        .x(function (d) { return x(d.RecvDate); })
        .y(function (d) { return y(d.Value); })
        .interpolate("linear");



    var svg = d3.select("#AmperageGraph")
        .attr("width", width + margin.left + margin.right)
        .attr("height", height + margin.top + margin.bottom)
        .append("g")
        .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

    var ln = svg.append("path").attr("class", "line");
    


    var x_axis = svg.append("g")
        .attr("class", "x axis")
        .attr("transform", "translate(0," + height + ")");
        

    var y_axis = svg.append("g")
            .attr("class", "y axis");


    x_axis.call(xAxis);
    y_axis.call(yAxis);



    var unicom = angular.module("unicom", []);

    unicom.controller('UnicomController', ['$scope', '$interval', 'requests', function UnicomController($scope, $interval, requests) {
        $scope.EWdata = {
            startDate: new Date(),
            endDate: new Date(),
            data: [],
            dataGraph: [],
        };

        // номинальное значение мощности и коэффициенты определяющие перегрузку или сухой ход
        $scope.NominalValues = {
            KoefUndo: 0,
            KoefOver: 0,
            NominalPower: 0
        };

        // предельные значение тока
        $scope.Ilimit = {
            min: localStorage.getItem("Ilimit.min"),
            max: localStorage.getItem("Ilimit.max")
        };

        // вешаем слушатель на изменение переменной - записываем в localstorage
        $scope.$watch('Ilimit.min', function (newValue, oldValue, scope) {
                if (newValue === oldValue) {
                    return;
                };
                localStorage.setItem("Ilimit.min", newValue);
        });

        // вешаем слушатель на изменение переменной - записываем в localstorage
        $scope.$watch('Ilimit.max', function (newValue, oldValue, scope) {
                if (newValue === oldValue) {
                    return;
                };
                localStorage.setItem("Ilimit.max", newValue);
            });

        // предельные значение напряжения
        $scope.Ulimit = {        
            min: localStorage.getItem("Ulimit.min"),
            max: localStorage.getItem("Ulimit.max")
        };

        // вешаем слушатель на изменение переменной - записываем в localstorage
        $scope.$watch('Ulimit.min', function (newValue, oldValue, scope) {
            if (newValue === oldValue) {
                return;
            };
            localStorage.setItem("Ulimit.min", newValue);
        });

        // вешаем слушатель на изменение переменной - записываем в localstorage
        $scope.$watch('Ulimit.max', function (newValue, oldValue, scope) {
            if (newValue === oldValue) {
                return;
            };
            localStorage.setItem("Ulimit.max", newValue);
        });

        // предельные значения мощности
        $scope.Wlimit = {
            min: localStorage.getItem("Wlimit.min"),
            max: localStorage.getItem("Wlimit.max")
        };

        // вешаем слушатель на изменение переменной - записываем в localstorage
        $scope.$watch('Wlimit.min', function (newValue, oldValue, scope) {
            if (newValue === oldValue) {
                return;
            };
            localStorage.setItem("Wlimit.min", newValue);
        });

        // вешаем слушатель на изменение переменной - записываем в localstorage
        $scope.$watch('Wlimit.max', function (newValue, oldValue, scope) {
            if (newValue === oldValue) {
                return;
            };
            localStorage.setItem("Wlimit.max", newValue);
        });

        // предельные значения давления
        $scope.Plimit = {
            min: localStorage.getItem("Plimit.min"),
            max: localStorage.getItem("Plimit.max")
        };

        // вешаем слушатель на изменение переменной - записываем в localstorage
        $scope.$watch('Plimit.min', function (newValue, oldValue, scope) {
            if (newValue === oldValue) {
                return;
            };
            localStorage.setItem("Plimit.min", newValue);
        });

        // вешаем слушатель на изменение переменной - записываем в localstorage
        $scope.$watch('Plimit.max', function (newValue, oldValue, scope) {
            if (newValue === oldValue) {
                return;
            };
            localStorage.setItem("Plimit.max", newValue);
        });

        // имя параметра по которому строиться график
        $scope.graphParam = 'TotalEnergy';

        $scope.updatedTime = null;

        // вычисляю среднее значение показаний эл энергии
        $scope.avgEnergy = function () {
            if ($scope.EWdata.data.length == 0)
                return 0;
            var calculated = 0;
            var count = 0;

            for (var i = 0; i < $scope.EWdata.data.length; i++) {
                if ($scope.EWdata.data[i].TotalEnergy > 0) {
                    calculated += $scope.EWdata.data[i].TotalEnergy;
                    count++;
                }
            }
            if(count>0)
            {
                return calculated / count;
            }else
            {
                return 0;
            }
        }

        // рассчиываю расход воды часовой на основе данных за посл 10 мин
        $scope.WaterByHour = function () {
            // интервал
            var to_ = moment($scope.updatedTime);
            var from_ = moment($scope.updatedTime).subtract(10, 'minutes');

            // выбираю записи из интервала за 10 мин
            var dataCalculate = $scope.EWdata.data.filter(function (e,i) {
                return e.RecvDate >= from_ && e.RecvDate <= to_ && e.TotalWaterRate > 0;
            });

            var substractValue = 0;
            var secondsInterval = 0;
            // если данных > чем 2 записи, то рассчитываю на основе первого и последнего значения из интервала
            if (dataCalculate.length > 0) {
                // разность показаний
                substractValue = dataCalculate[0].WaterCalculated - dataCalculate[dataCalculate.length - 1].WaterCalculated;
                // разность в секундах
                secondsInterval = moment(dataCalculate[0].RecvDate).diff(moment(dataCalculate[dataCalculate.length - 1].RecvDate), "seconds");

                if (secondsInterval != 0) {
                    return substractValue * 60 * 60 / secondsInterval;
                } else
                    return 0;
                // нет данных за последние 10 мин
            } else {
                return 0;
            }
        
        };

        // посчитаный расход воды по табличным данным
        $scope.WaterRate = function() {
            var filtered = $scope.EWdata.data.filter(function (e, i) { // фильтрую данные по >0 импульсов
                return e.TotalWaterRate > 0;
            });
            // расход беру по вычисленному значению
            if (filtered.length > 0) {
                return filtered[0].WaterCalculated - filtered[filtered.length - 1].WaterCalculated;
            } else {
                return 0;
            }

        };
    
        $scope.EnergyRate = function () {
            if ($scope.EWdata.data.length > 0) {
                return $scope.EWdata.data[0].TotalEnergy - $scope.EWdata.data[$scope.EWdata.data.length - 1].TotalEnergy;
            } else {
                return 0;
            }
        };

        // время в минутах
        $scope.intervalData = function() {
            if ($scope.EWdata.data.length > 0) {
                return moment.duration($scope.EWdata.data[0].RecvDate - $scope.EWdata.data[$scope.EWdata.data.length - 1].RecvDate).humanize();///1000/60;
            } else {
                return 0;
            }
        };
    
        // последние показания
        $scope.lastData = function() {
            if ($scope.EWdata.data.length > 0) {
                return $scope.EWdata.data[0];
            } else {
                return {
                
                    Id:null,
                    RecvDate: null,
                    TotalEnergy: null,
                    Amperage1: null,
                    Amperage2: null,
                    Amperage3: null,
                    Voltage1: null,
                    Voltage2: null,
                    Voltage3: null,
                    CurrentElectricPower: null,
                    TotalWaterRate: null,
                    WaterEnergy: null,
                    Frequrency: null,
                    Presure: null,
                    Temperature: null,
                    WaterKoef: null,
                    WaterStartValue: null,
                    WaterCalculated:null,
                    Errors: null,
                    EngineAmp:null
                };
            }
        };

        // время, прошедшее после получения последних данных (мин)
        $scope.loseDataTime = function () {
            
            return ($scope.updatedTime - $scope.lastData().RecvDate) / 1000/60;
        };
   
        $scope.init = function (identity) {
            $scope.loadData(identity);
            
            requests.getPumpNominal(identity)
                .success(function (data) {
                    $scope.NominalValues.KoefOver = data.KoefOver;
                    $scope.NominalValues.KoefUndo = data.KoefUndo;
                    $scope.NominalValues.NominalPower = data.NominalPower;
                })
                .error(function (data, status, headers, config) { console.error("ошибка при получении справочных номинальных параметров мощности и коеффициентов"+data); })
                .finally(function () {});
       
            $interval(function () { $scope.loadData(identity); }, 10000);
              
        };

        // запрос данных
        $scope.loadData = function (identity) {
            showLoading(true);
            requests.getLastData(identity, $scope.graphParam)
                .success(function (data, status, headers, config) {
                    $scope.EWdata.startDate = new Date(data.StartDate);  //parseInt(data.StartDate.substr(6))
                    $scope.EWdata.endDate = new Date(data.EndDate);
                
                
                    $scope.EWdata.data = [];
                    data.DataTable.map(function(e, i) {
                        $scope.EWdata.data.push({
                            Id: e.Id,
                            RecvDate: new Date(e.dt),  //parseInt(e.RecvDate.substr(6))
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
                    $scope.EWdata.dataGraph = [];
                    data.DataGraph.map(function(e, i) {
                        $scope.EWdata.dataGraph.push({
                            RecvDate: new Date(e.RecvDate), //parseInt(e.RecvDate.substr(6))
                            Value: e.Value
                        });
                    });
                   

                    x.domain([$scope.EWdata.startDate, $scope.EWdata.endDate]);
                    y.domain([d3.min($scope.EWdata.dataGraph,function(d){return d.Value;})*0.99,d3.max($scope.EWdata.dataGraph,function(d){return d.Value;})*1.01]);  // d3.extent($scope.EWdata.dataGraph, function (d) { return d.Value; }));
                    
                    x_axis.call(xAxis);
                    y_axis.call(yAxis);
                    
                                           
                    

                    ln.datum($scope.EWdata.dataGraph)
                       .attr("d", line);
                    
                    var dots = svg.selectAll("circle.dot").data($scope.EWdata.dataGraph);

                    

                    dots.enter()
                        .append("circle")
                        .attr("class", "dot")
                        .attr("cx", function(d) { return x(d.RecvDate); })
                        .attr("cy", function(d) { return y(d.Value); })
                        .attr("r", 0.8);
                    
                    dots.attr("cx", function (d) { return x(d.RecvDate); })
                        .attr("cy", function (d) { return y(d.Value); });
                        
                    dots.exit().remove();

                
                
                }).error(function (data, status, headers, config) { console.error("ошибка при выполнении запроса на сервер"+data); })
                .finally(function () {
                    $scope.updatedTime = new Date();
                    showLoading(false);
                });
        };

        // прототип функции анализирующей сухой ход насоса
        $scope.isAlarm = function (nums) {
            var result = {
                show: false,
                message: '',
                cls:''
            }
            // если данных меньше чем нужно проанализировать не показываем сообщение
            if ($scope.EWdata.data.length < nums) {
                result.show = false;
                result.message = '';
                result.cls = '';

                return result;
            }
            var isHighPower = true && $scope.NominalValues.KoefOver > 0, isSuhHod = true && $scope.NominalValues.KoefUndo>0;
            //water = $scope.EWdata.data[0].WaterCalculated - $scope.EWdata.data[nums - 1].WaterCalculated;
            if ($scope.NominalValues.NominalPower > 0) {
                for (var i = 0; i < nums; i++) {
                    isHighPower = isHighPower && $scope.EWdata.data[i].CurrentElectricPower > $scope.NominalValues.NominalPower * (1 + $scope.NominalValues.KoefOver / 100.0);
                    isSuhHod = isSuhHod && $scope.EWdata.data[i].CurrentElectricPower < $scope.NominalValues.NominalPower * (1 - $scope.NominalValues.KoefUndo / 100.0);
                }
                if (isHighPower || isSuhHod) {
                    result.show = true;
                    result.cls = 'alarmDanger';
                    if(isHighPower)
                        result.message = 'Зафиксировано превышение номинальной мощности насоса!';
                    if(isSuhHod)
                        result.message+= ' Анализ мощности показал высокую вероятность "сухого хода" насоса!';
                }else
                {
                    result.show = false;
                    result.message = '';
                    result.cls = '';
                }
            }

            return result;
        };

        $scope.IclassLimit = function(value,limit) {
            if (value && (limit.min && value < limit.min || limit.max&&value > limit.max)) {
                return "err-val";
            }
            return "";
        };

        // класс для значения с полем с ошибкой
        $scope.getClassRowError = function(elem) {
            if (elem != null && elem.length > 0) {
                return 'error_field';
            }
            return '';
        };

    }]);


    unicom.service("requests", ['$http',function ($http) {
        this.getLastData = function(identity,type) {
            return $http({
                url: '../GetDataBySmallPeriod',
                method: 'GET',
                dataType: 'json',
                cache: false,
                params: { 'identity': identity, 'parameterGraph': type }
            });
        };

        this.getPumpNominal = function (identity) {
            return $http({
                url: '/Water/PumpNominalParameters',
                method: 'POST',
                dataType: 'json',
                cache: false,
                params: { 'identity': identity }
            });
            
        };
    }]);

    unicom.directive("alert", function () {
        return {
            restrict: 'E',
            transclude: true,
            template: '<div class="alert_ col-md-12 col-sm-12 col-xs-12 {{cls}}">{{msg}}</div>',
            scope:{
                cls: "@",
                msg:"@"
            },
            link: function (scope, element, attrs) {
                element;
            }
        }
    });