var app = angular.module('VotingApp', []);
app.run(function () { });

app.controller('VotingAppController', ['$rootScope', '$scope', '$http', function ($rootScope, $scope, $http) {

    $scope.refresh = function () {
        $http.get('api/Votes')
            .then(function (data) {
                $scope.votes = data;
            });
    };

    $scope.remove = function (item) {
        $http.delete('api/Votes/' + item)
            .then(function() {
                $scope.refresh();
            });
    };

    $scope.add = function (item) {
        $http.put('api/Votes/' + item)
            .then(function () {
                $scope.refresh();
                $scope.item = undefined;
            });
    };
}]);
