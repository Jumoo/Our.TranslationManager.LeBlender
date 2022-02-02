(function () {

    'use strict';

    function summaryArchiveController(translateJobService, $location, $timeout, navigationService) {

        var vm = this;

        vm.pageTitle = 'Archived Jobs';
        vm.loading = true;

        vm.viewJobs = viewJobs;
        vm.statusRange = [20, 100];

        loadJobInfo();

        $timeout(function () {
            navigationService.syncTree({ tree: "archive", path: ['-1'] });
        });


        ///////////////
        function loadJobInfo() {
            translateJobService.getSummaryRange(20, 100)
                .then(function (result) {
                    vm.info = result.data;
                    vm.loading = false;
                });
        }

        function viewJobs(id) {
            $location.path('/translation/archive/list/' + id);
        }
    }

    angular.module('umbraco')
        .controller('translateSummaryArchiveController', summaryArchiveController);

})();