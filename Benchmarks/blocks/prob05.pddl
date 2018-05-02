;;;
;;; The blocks problem!
;;;
(define (problem 05)
  (:domain blocks)
  (:objects boid - steeringagent
            l4 l3 l2 l1 l0 - location
			block1 block2 - block)
  (:init (at block1 l4)
		 (at block2 l3)
		 (at boid l0)
		 (freehands boid)
		 (occupied l0)
		 (occupied l3)
		 (occupied l4)
		 (adjacent l0 l1)
		 (adjacent l1 l0)
		 (adjacent l1 l2)
		 (adjacent l2 l1)
		 (adjacent l1 l3)
		 (adjacent l3 l1)
		 (adjacent l3 l4)
		 (adjacent l4 l3)
		)
  (:goal (at block1 l0)))